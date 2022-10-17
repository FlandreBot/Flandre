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

/// <summary>
/// Konata Bot
/// </summary>
public class KonataBot : IBot
{
    /// <summary>
    /// Bot 平台名称，值为 konata
    /// </summary>
    public string Platform => "konata";
    
    /// <summary>
    /// Konata 内部 bot
    /// </summary>
    public Bot Internal { get; }

    private readonly Logger _logger;
    private readonly KonataBotConfig _config;

    /// <summary>
    /// 消息接收事件
    /// </summary>
    public event IBot.BotEventHandler<BotMessageReceivedEvent>? OnMessageReceived;

    /// <summary>
    /// 邀请入群事件
    /// </summary>
    public event IBot.BotEventHandler<BotGuildInvitedEvent>? OnGuildInvited;

    /// <summary>
    /// 加群事件
    /// </summary>
    public event IBot.BotEventHandler<BotGuildRequestedEvent>? OnGuildRequested;

    /// <summary>
    /// 好友申请事件
    /// </summary>
    public event IBot.BotEventHandler<BotFriendRequestedEvent>? OnFriendRequested;

    #region 事件处理

    /// <summary>
    /// 处理拉群邀请
    /// </summary>
    /// <param name="e">拉群邀请事件</param>
    /// <param name="approve">是否同意</param>
    /// <param name="comment">附加说明</param>
    public async Task HandleGuildInvitation(BotGuildInvitedEvent e, bool approve, string? comment = null)
    {
        if (approve)
            await Internal.ApproveGroupInvitation(
                uint.Parse(e.GuildId), uint.Parse(e.InviterId), (long)e.EventMessage!);
        else
            await Internal.DeclineGroupInvitation(
                uint.Parse(e.GuildId), uint.Parse(e.InviterId), (long)e.EventMessage!, comment ?? "");
    }

    /// <summary>
    /// 处理加群申请
    /// </summary>
    /// <param name="e">加群申请事件</param>
    /// <param name="approve">是否同意</param>
    /// <param name="comment">附加说明</param>
    public async Task HandleGuildRequest(BotGuildRequestedEvent e, bool approve, string? comment = null)
    {
        if (approve)
            await Internal.ApproveGroupRequestJoin(
                uint.Parse(e.GuildId), uint.Parse(e.RequesterId), (long)e.EventMessage!);
        else
            await Internal.DeclineGroupRequestJoin(
                uint.Parse(e.GuildId), uint.Parse(e.RequesterId), (long)e.EventMessage!, comment ?? "");
    }

    /// <summary>
    /// 处理好友申请
    /// </summary>
    /// <param name="e">好友申请事件</param>
    /// <param name="approve">是否同意</param>
    /// <param name="comment">附加说明</param>
    public async Task HandleFriendRequest(BotFriendRequestedEvent e, bool approve, string? comment = null)
    {
        if (approve)
            await Internal.ApproveFriendRequest(
                uint.Parse(e.RequesterId), (long)e.EventMessage!);
        else
            await Internal.DeclineFriendRequest(
                uint.Parse(e.RequesterId), (long)e.EventMessage!);
    }

    #endregion

    internal KonataBot(KonataBotConfig config, Logger logger)
    {
        Internal = BotFather.Create(config.Konata, config.Device, config.KeyStore);
        _config = config;
        _logger = logger;

        Internal.OnFriendMessage += InnerOnFriendMessage;
        Internal.OnGroupMessage += InnerOnGroupMessage;
        Internal.OnGroupInvite += InnerOnGroupInvite;
        Internal.OnGroupRequestJoin += InnerOnGroupRequestJoin;
        Internal.OnFriendRequest += InnerOnFriendRequest;

        Internal.OnCaptcha += InnerOnCaptcha;
        Internal.OnLog += InnerOnLog;
    }

    #region 生命周期

    /// <summary>
    /// 启动 bot
    /// </summary>
    public async Task Start()
    {
        _logger.Info("Starting Konata Bot...");
        if (!await Internal.Login())
        {
            _logger.Warning($"{_config.SelfId} 登陆失败。");
            return;
        }

        _config.KeyStore = Internal.KeyStore;
        _logger.Info("Konata Bot started.");
    }

    /// <summary>
    /// 停止 bot
    /// </summary>
    public Task Stop()
    {
        return Task.Run(() => Internal.Dispose());
    }

    #endregion 生命周期

    #region 消息相关

    /// <inheritdoc />
    public Task<string?> SendMessage(MessageSourceType sourceType, string? channelId, string? userId,
        MessageContent content, string? guildId = null)
    {
        return sourceType switch
        {
            MessageSourceType.Channel => SendChannelMessage(channelId!, content),
            MessageSourceType.Private => SendPrivateMessage(userId!, content),
            _ => Task.FromResult<string?>(null)
        };
    }

    /// <inheritdoc />
    public Task<string?> SendMessage(Message message, MessageContent? contentOverride = null)
    {
        return SendMessage(message.SourceType, message.ChannelId, message.Sender.UserId,
            contentOverride ?? message.Content);
    }

    /// <summary>
    /// 发送群消息
    /// </summary>
    /// <param name="channelId">群号</param>
    /// <param name="content">消息内容</param>
    /// <param name="guildId"></param>
    public async Task<string?> SendChannelMessage(string channelId, MessageContent content, string? guildId = null)
    {
        await Internal.SendGroupMessage(
            uint.Parse(channelId), content.ToKonataMessageChain());
        return null;
    }

    /// <summary>
    /// 发送私聊消息
    /// </summary>
    /// <param name="userId">用户 QQ 号</param>
    /// <param name="content">消息内容</param>
    public async Task<string?> SendPrivateMessage(string userId, MessageContent content)
    {
        await Internal.SendFriendMessage(
            uint.Parse(userId), content.ToKonataMessageChain());
        return null;
    }

    /// <remarks>Konata 平台不支持该操作。</remarks>
    public Task DeleteMessage(string messageId)
    {
        return Task.CompletedTask;
    }

    #endregion 消息相关

    #region 用户相关

    /// <summary>
    /// 获取自身信息
    /// </summary>
    public Task<User> GetSelf()
    {
        return Task.FromResult(new User
        {
            Name = Internal.Name,
            Nickname = Internal.Name,
            UserId = Internal.Uin.ToString(),
            AvatarUrl = CommonUtils.GetAvatarUrl(Internal.Uin)
        });
    }

    /// <inheritdoc />
    public async Task<User?> GetUser(string userId, string? guildId = null)
    {
        return (await GetFriendList()).FirstOrDefault(user => user.UserId == userId);
    }

    /// <summary>
    /// 获取好友列表
    /// </summary>
    public async Task<IEnumerable<User>> GetFriendList()
    {
        return (await Internal.GetFriendList(true)).Select(friend => new User
        {
            Name = friend.Name,
            Nickname = friend.Remark,
            UserId = friend.Uin.ToString(),
            AvatarUrl = CommonUtils.GetAvatarUrl(friend.Uin)
        });
    }

    #endregion 用户相关

    #region Guild 相关

    /// <summary>
    /// 获取群信息
    /// </summary>
    /// <param name="guildId">群号</param>    
    public async Task<Guild?> GetGuild(string guildId)
    {
        return (await GetGuildList()).FirstOrDefault(guild => guild.Id == guildId);
    }

    /// <summary>
    /// 获取 Bot 加入的群列表
    /// </summary>
    public async Task<IEnumerable<Guild>> GetGuildList()
    {
        return (await Internal.GetGroupList(true))
            .Select(group => new Guild
            {
                Id = group.Uin.ToString(),
                Name = group.Name
            });
    }

    /// <summary>
    /// 获取群成员信息
    /// </summary>
    /// <param name="guildId">群号</param>
    /// <param name="userId">群成员 QQ</param>
    public async Task<GuildMember?> GetGuildMember(string guildId, string userId)
    {
        return (await GetGuildMemberList(guildId)).FirstOrDefault(member => member.UserId == userId);
    }

    /// <summary>
    /// 获取群成员列表
    /// </summary>
    /// <param name="guildId">群号</param>
    public async Task<IEnumerable<GuildMember>> GetGuildMemberList(string guildId)
    {
        return (await Internal.GetGroupMemberList(uint.Parse(guildId), true))
            .Select(member => new GuildMember
            {
                Name = member.Name,
                Nickname = member.NickName,
                UserId = member.Uin.ToString(),
                AvatarUrl = CommonUtils.GetAvatarUrl(member.Uin),
                Roles = new List<string> { member.Role.ToString() }
            });
    }

    #endregion Guild 相关

    #region Channel 相关

    /// <summary>
    /// 获取群列表，对于 Konata 等效于 <see cref="GetGuild"/>
    /// </summary>
    public async Task<Channel?> GetChannel(string channelId, string? guildId = null)
    {
        return (await GetChannelList("")).FirstOrDefault(channel => channel.Id == channelId);
    }

    /// <summary>
    /// 获取群列表，对于 Konata 等效于 <see cref="GetGuildList"/>
    /// </summary>
    public async Task<IEnumerable<Channel>> GetChannelList(string guildId)
    {
        return (await Internal.GetGroupList(true))
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
            e.InviterNick, e.InviterUin.ToString(), e.InviterIsAdmin)
        {
            EventMessage = e.Token
        });
    }

    private void InnerOnGroupRequestJoin(Bot bot, GroupRequestJoinEvent e)
    {
        OnGuildRequested?.Invoke(this, new BotGuildRequestedEvent(
            e.GroupName, e.GroupUin.ToString(),
            e.ReqNick, e.ReqUin.ToString(), e.ReqComment)
        {
            EventMessage = e.Token
        });
    }

    private void InnerOnFriendRequest(Bot bot, FriendRequestEvent e)
    {
        OnFriendRequested?.Invoke(this, new BotFriendRequestedEvent(
            e.ReqNick, e.ReqUin.ToString(), e.ReqComment)
        {
            EventMessage = e.Token
        });
    }

    private void InnerOnCaptcha(Bot bot, CaptchaEvent e)
    {
        _logger.Warning($"Bot {_config.SelfId} 需要进行登录验证。");

        switch (e.Type)
        {
            case CaptchaEvent.CaptchaType.Sms:
                _logger.Warning($"手机验证码已发送至 {e.Phone}，请注意查收，输入验证码后按 Enter 继续：");
                Internal.SubmitSmsCode(Console.ReadLine());
                break;

            case CaptchaEvent.CaptchaType.Slider:
                _logger.Warning($"滑动验证码 URL: {e.SliderUrl}，输入 Ticket 后按 Enter 继续：");
                Internal.SubmitSliderTicket(Console.ReadLine());
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

/// <summary>
/// Konata bot 配置
/// </summary>
public class KonataBotConfig : BotConfig
{
    /// <summary>
    /// Konata 内部 bot 配置
    /// </summary>
    public global::Konata.Core.Common.BotConfig Konata { get; set; } = global::Konata.Core.Common.BotConfig.Default();

    /// <summary>
    /// Konata 设备信息
    /// </summary>
    public BotDevice Device { get; set; } = BotDevice.Default();

    /// <summary>
    /// Konata 密钥信息
    /// </summary>
    public BotKeyStore KeyStore { get; set; } = new();
}