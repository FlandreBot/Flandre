using Flandre.Core.Common;
using Flandre.Core.Events.Bot;
using Flandre.Core.Messaging;
using Flandre.Core.Models;
using Flandre.Core.Utils;
using Konata.Core;
using Konata.Core.Common;
using Konata.Core.Events;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces;
using Konata.Core.Interfaces.Api;
using BotConfig = Flandre.Core.Common.BotConfig;

namespace Flandre.Adapters.Konata;

public class KonataBot : IBot
{
    public Bot InnerBot { get; }

    private readonly Logger _logger;
    private readonly KonataBotConfig _config;

    public event IBot.BotEventHandler<BotMessageReceivedEvent>? OnMessageReceived;
    public event IBot.BotEventHandler<BotGuildInvitedEvent>? OnGuildInvited;
    public event IBot.BotEventHandler<BotGuildRequestedEvent>? OnGuildRequested;
    public event IBot.BotEventHandler<BotFriendRequestedEvent>? OnFriendRequested;

    internal KonataBot(KonataBotConfig config, Logger logger)
    {
        InnerBot = BotFather.Create(config.Konata, config.Device, config.KeyStore);
        _config = config;
        _logger = logger;

        InnerBot.OnFriendMessage += InnerOnFriendMessage;
        InnerBot.OnGroupMessage += InnerOnGroupMessage;
        InnerBot.OnGroupInvite += InnerOnGroupInvite;
        InnerBot.OnGroupRequestJoin += InnerOnGroupRequestJoin;
        InnerBot.OnFriendRequest += InnerOnFriendRequest;

        InnerBot.OnCaptcha += InnerOnCaptcha;
        InnerBot.OnLog += InnerOnLog;
    }

    #region 生命周期

    public async Task Start()
    {
        _logger.Info("Starting Konata Bot...");
        if (!await InnerBot.Login())
        {
            _logger.Warning($"{_config.SelfId} 登陆失败。");
            return;
        }

        _config.KeyStore = InnerBot.KeyStore;
        _logger.Info("Konata Bot started.");
    }

    public Task Stop()
    {
        return Task.Run(() => InnerBot.Dispose());
    }

    #endregion 生命周期

    #region 发送消息

    public Task SendMessage(MessageSourceType sourceType, string guildId, string channelId, string userId,
        MessageContent content)
    {
        return sourceType switch
        {
            MessageSourceType.Channel => SendChannelMessage(guildId, channelId, content),
            MessageSourceType.Private => SendPrivateMessage(userId, content),
            _ => Task.CompletedTask
        };
    }

    public Task SendMessage(Message message)
    {
        return SendMessage(message.SourceType, message.GuildId, message.ChannelId, message.Sender.Id, message.Content);
    }

    public async Task SendChannelMessage(string guildId, string channelId, MessageContent content)
    {
        await InnerBot.SendGroupMessage(
            uint.Parse(channelId), content.ToKonataMessageChain());
    }


    public async Task SendPrivateMessage(string userId, MessageContent content)
    {
        await InnerBot.SendFriendMessage(
            uint.Parse(userId), content.ToKonataMessageChain());
    }

    #endregion 发送消息

    #region 用户相关

    public Task<User> GetSelf()
    {
        return Task.FromResult(new User
        {
            Name = InnerBot.Name,
            Nickname = InnerBot.Name,
            Id = InnerBot.Uin.ToString(),
            AvatarUrl = CommonUtils.GetAvatarUrl(InnerBot.Uin)
        });
    }

    public async Task<User?> GetUser(string userId)
    {
        return (await GetFriendList()).FirstOrDefault(user => user.Id == userId);
    }

    public async Task<IEnumerable<User>> GetFriendList()
    {
        return (await InnerBot.GetFriendList(true)).Select(friend => new User
        {
            Name = friend.Name,
            Nickname = friend.Remark,
            Id = friend.Uin.ToString(),
            AvatarUrl = CommonUtils.GetAvatarUrl(friend.Uin)
        });
    }

    #endregion 用户相关

    #region Guild 相关

    /// <remarks>由于 Konata 暂不支持 QQ 频道，该方法将固定返回 null。</remarks>
    public Task<Guild?> GetGuild(string guildId)
    {
        return Task.FromResult<Guild?>(null);
    }

    /// <remarks>由于 Konata 暂不支持 QQ 频道，该方法将固定返回空数组。</remarks>
    public Task<IEnumerable<Guild>> GetGuildList()
    {
        return Task.FromResult<IEnumerable<Guild>>(Array.Empty<Guild>());
    }

    /// <summary>
    ///     获取群成员信息
    /// </summary>
    /// <param name="guildId">群号</param>
    /// <param name="userId">群成员 QQ</param>
    public async Task<GuildMember?> GetGuildMember(string guildId, string userId)
    {
        return (await GetGuildMemberList(guildId)).FirstOrDefault(member => member.Id == userId);
    }

    /// <summary>
    ///     获取群成员列表
    /// </summary>
    /// <param name="guildId">群号</param>
    public async Task<IEnumerable<GuildMember>> GetGuildMemberList(string guildId)
    {
        return (await InnerBot.GetGroupMemberList(uint.Parse(guildId), true))
            .Select(member => new GuildMember
            {
                Name = member.Name,
                Nickname = member.NickName,
                Id = member.Uin.ToString(),
                AvatarUrl = CommonUtils.GetAvatarUrl(member.Uin),
                Roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { member.Role.ToString() }
            });
    }

    #endregion Guild 相关

    #region Channel 相关

    public async Task<Channel?> GetChannel(string channelId)
    {
        return (await GetChannelList()).FirstOrDefault(channel => channel.Id == channelId);
    }

    public async Task<IEnumerable<Channel>> GetChannelList()
    {
        return (await InnerBot.GetGroupList(true))
            .Select(group => new Channel
            {
                Id = group.Uin.ToString(),
                Name = group.Name
            });
    }

    #endregion Channel 相关

    #region 事件

    private void InnerOnFriendMessage(Bot bot, FriendMessageEvent e)
    {
        OnMessageReceived?.Invoke(this, new BotMessageReceivedEvent(e.Message.ToFlandreMessage()));
    }

    private void InnerOnGroupMessage(Bot bot, GroupMessageEvent e)
    {
        OnMessageReceived?.Invoke(this, new BotMessageReceivedEvent(e.Message.ToFlandreMessage()));
    }

    private void InnerOnGroupInvite(Bot bot, GroupInviteEvent e)
    {
        OnGuildInvited?.Invoke(this, new BotGuildInvitedEvent(
            e.GroupName, e.GroupUin.ToString(),
            e.InviterNick, e.InviterUin.ToString(), e.InviterIsAdmin));
    }

    private void InnerOnGroupRequestJoin(Bot bot, GroupRequestJoinEvent e)
    {
        OnGuildRequested?.Invoke(this, new BotGuildRequestedEvent(
            e.GroupName, e.GroupUin.ToString(),
            e.ReqNick, e.ReqUin.ToString(), e.ReqComment));
    }

    private void InnerOnFriendRequest(Bot bot, FriendRequestEvent e)
    {
        OnFriendRequested?.Invoke(this, new BotFriendRequestedEvent(
            e.ReqNick, e.ReqUin.ToString(), e.ReqComment));
    }

    private void InnerOnCaptcha(Bot bot, CaptchaEvent e)
    {
        _logger.Warning($"Bot {_config.SelfId} 需要进行登录验证。");

        switch (e.Type)
        {
            case CaptchaEvent.CaptchaType.Sms:
                _logger.Warning($"手机验证码已发送至 {e.Phone}，请注意查收，输入验证码后按 Enter 继续：");
                InnerBot.SubmitSmsCode(Console.ReadLine());
                break;

            case CaptchaEvent.CaptchaType.Slider:
                _logger.Warning($"滑动验证码 URL: {e.SliderUrl}，输入 Ticket 后按 Enter 继续：");
                InnerBot.SubmitSliderTicket(Console.ReadLine());
                break;
        }
    }

    private void InnerOnLog(Bot bot, LogEvent e)
    {
        switch (e.Level)
        {
            case LogLevel.Warning:
                _logger.Warning(e.EventMessage);
                break;

            case LogLevel.Exception:
            case LogLevel.Fatal:
                _logger.Error(e.EventMessage);
                break;
        }
    }

    #endregion 事件
}

public class KonataBotConfig : BotConfig
{
    public global::Konata.Core.Common.BotConfig Konata { get; set; } = global::Konata.Core.Common.BotConfig.Default();

    public BotDevice Device { get; set; } = BotDevice.Default();

    public BotKeyStore KeyStore { get; set; } = new();
}