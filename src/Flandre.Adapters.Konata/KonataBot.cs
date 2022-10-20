using Flandre.Core.Events.Bot;
using Flandre.Core.Messaging;
using Flandre.Core.Models;
using Flandre.Core.Utils;
using Konata.Core.Common;
using Konata.Core.Events;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces;
using Konata.Core.Interfaces.Api;
using FlandreBotConfig = Flandre.Core.Common.BotConfig;
using FlandreBot = Flandre.Core.Common.Bot;
using KonataInternalBot = Konata.Core.Bot;

namespace Flandre.Adapters.Konata;

/// <summary>
/// Konata Bot
/// </summary>
public sealed class KonataBot : FlandreBot
{
    /// <summary>
    /// Bot 平台名称，值为 konata
    /// </summary>
    public override string Platform => _config.PlatformOverride ?? "konata";

    /// <summary>
    /// Konata 内部 bot
    /// </summary>
    public KonataInternalBot Internal { get; }

    private readonly Logger _logger;
    private readonly KonataBotConfig _config;

    /// <inheritdoc />
    protected override Logger GetLogger() => _logger;

    /// <inheritdoc />
    public override event BotEventHandler<BotMessageReceivedEvent>? OnMessageReceived;

    /// <inheritdoc />
    public override event BotEventHandler<BotGuildInvitedEvent>? OnGuildInvited;

    /// <inheritdoc />
    public override event BotEventHandler<BotGuildJoinRequestedEvent>? OnGuildJoinRequested;

    /// <inheritdoc />
    public override event BotEventHandler<BotFriendRequestedEvent>? OnFriendRequested;

    internal KonataBot(KonataBotConfig config, Logger logger)
    {
        _logger = logger;
        Internal = BotFather.Create(config.Konata, config.Device, config.KeyStore);
        _config = config;

        Internal.OnFriendMessage += InnerOnFriendMessage;
        Internal.OnGroupMessage += InnerOnGroupMessage;
        Internal.OnGroupInvite += InnerOnGroupInvite;
        Internal.OnGroupRequestJoin += InnerOnGroupRequestJoin;
        Internal.OnFriendRequest += InnerOnFriendRequest;

        Internal.OnCaptcha += InnerOnCaptcha;
        Internal.OnLog += InnerOnLog;
    }

    #region 生命周期

    /// <inheritdoc />
    public override async Task Start()
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

    /// <inheritdoc />
    public override Task Stop() => Task.Run(() => Internal.Dispose());

    #endregion 生命周期

    #region 消息相关

    /// <summary>
    /// 发送群消息
    /// </summary>
    /// <param name="channelId">群号</param>
    /// <param name="content">消息内容</param>
    /// <param name="guildId">群号，可不提供</param>
    public override async Task<string?> SendChannelMessage(string channelId, MessageContent content,
        string? guildId = null)
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
    public override async Task<string?> SendPrivateMessage(string userId, MessageContent content)
    {
        await Internal.SendFriendMessage(
            uint.Parse(userId), content.ToKonataMessageChain());
        return null;
    }

    #endregion 消息相关

    #region 用户相关

    /// <inheritdoc />
    public override Task<User?> GetSelf()
    {
        return Task.FromResult<User?>(new User
        {
            Name = Internal.Name,
            UserId = Internal.Uin.ToString(),
            AvatarUrl = CommonUtils.GetAvatarUrl(Internal.Uin)
        });
    }

    /// <inheritdoc />
    /// <remarks>在 Konata 中只能获取好友的信息。</remarks>
    public override async Task<User?> GetUser(string userId, string? guildId = null)
    {
        return (await GetFriendList()).FirstOrDefault(user => user.UserId == userId);
    }

    /// <inheritdoc />
    public override async Task<IEnumerable<User>> GetFriendList()
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
    public override async Task<Guild?> GetGuild(string guildId)
    {
        return (await GetGuildList()).FirstOrDefault(guild => guild.Id == guildId);
    }

    /// <summary>
    /// 获取 Bot 加入的群列表
    /// </summary>
    public override async Task<IEnumerable<Guild>> GetGuildList()
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
    public override async Task<GuildMember?> GetGuildMember(string guildId, string userId)
    {
        return (await GetGuildMemberList(guildId)).FirstOrDefault(member => member.UserId == userId);
    }

    /// <summary>
    /// 获取群成员列表
    /// </summary>
    /// <param name="guildId">群号</param>
    public override async Task<IEnumerable<GuildMember>> GetGuildMemberList(string guildId)
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
    public override async Task<Channel?> GetChannel(string channelId, string? guildId = null)
    {
        return (await GetChannelList("")).FirstOrDefault(channel => channel.Id == channelId);
    }

    /// <summary>
    /// 获取群列表，对于 Konata 等效于 <see cref="GetGuildList"/>
    /// </summary>
    public override async Task<IEnumerable<Channel>> GetChannelList(string guildId)
    {
        return (await Internal.GetGroupList(true))
            .Select(group => new Channel
            {
                Id = group.Uin.ToString(),
                Name = group.Name
            });
    }

    #endregion Channel 相关

    #region 内部事件

    private void InnerOnFriendMessage(KonataInternalBot bot, FriendMessageEvent e)
    {
        OnMessageReceived?.Invoke(this, new BotMessageReceivedEvent(e.Message.ToFlandreMessage()));
    }

    private void InnerOnGroupMessage(KonataInternalBot bot, GroupMessageEvent e)
    {
        OnMessageReceived?.Invoke(this, new BotMessageReceivedEvent(e.Message.ToFlandreMessage()));
    }

    private void InnerOnGroupInvite(KonataInternalBot bot, GroupInviteEvent e)
    {
        OnGuildInvited?.Invoke(this, new BotGuildInvitedEvent(
            e.GroupName, e.GroupUin.ToString(),
            e.InviterNick, e.InviterUin.ToString(), e.InviterIsAdmin)
        {
            EventMessage = e.Token
        });
    }

    private void InnerOnGroupRequestJoin(KonataInternalBot bot, GroupRequestJoinEvent e)
    {
        OnGuildJoinRequested?.Invoke(this, new BotGuildJoinRequestedEvent(
            e.GroupName, e.GroupUin.ToString(),
            e.ReqNick, e.ReqUin.ToString(), e.ReqComment)
        {
            EventMessage = e.Token
        });
    }

    private void InnerOnFriendRequest(KonataInternalBot bot, FriendRequestEvent e)
    {
        OnFriendRequested?.Invoke(this, new BotFriendRequestedEvent(
            e.ReqNick, e.ReqUin.ToString(), e.ReqComment)
        {
            EventMessage = e.Token
        });
    }

    private void InnerOnCaptcha(KonataInternalBot bot, CaptchaEvent e)
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

    private void InnerOnLog(KonataInternalBot bot, LogEvent e)
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

    #endregion 内部事件

    #region 事件处理

    /// <inheritdoc />
    public override async Task HandleGuildInvitation(BotGuildInvitedEvent e, bool approve, string? comment = null)
    {
        if (approve)
            await Internal.ApproveGroupInvitation(
                uint.Parse(e.GuildId), uint.Parse(e.InviterId), (long)e.EventMessage!);
        else
            await Internal.DeclineGroupInvitation(
                uint.Parse(e.GuildId), uint.Parse(e.InviterId), (long)e.EventMessage!, comment ?? "");
    }

    /// <inheritdoc />
    public override async Task HandleGuildJoinRequest(BotGuildJoinRequestedEvent e, bool approve,
        string? comment = null)
    {
        if (approve)
            await Internal.ApproveGroupRequestJoin(
                uint.Parse(e.GuildId), uint.Parse(e.RequesterId), (long)e.EventMessage!);
        else
            await Internal.DeclineGroupRequestJoin(
                uint.Parse(e.GuildId), uint.Parse(e.RequesterId), (long)e.EventMessage!, comment ?? "");
    }

    /// <inheritdoc />
    public override async Task HandleFriendRequest(BotFriendRequestedEvent e, bool approve, string? comment = null)
    {
        if (approve)
            await Internal.ApproveFriendRequest(
                uint.Parse(e.RequesterId), (long)e.EventMessage!);
        else
            await Internal.DeclineFriendRequest(
                uint.Parse(e.RequesterId), (long)e.EventMessage!);
    }

    #endregion
}

/// <summary>
/// Konata Bot 配置
/// </summary>
public class KonataBotConfig : FlandreBotConfig
{
    /// <summary>
    /// Konata 内部 Bot 配置
    /// </summary>
    public BotConfig Konata { get; set; } = BotConfig.Default();

    /// <summary>
    /// Konata 设备信息
    /// </summary>
    public BotDevice Device { get; set; } = BotDevice.Default();

    /// <summary>
    /// Konata 密钥信息
    /// </summary>
    public BotKeyStore KeyStore { get; set; } = new();

    /// <summary>
    /// 覆盖平台字符串。对于使用多个 QQ 适配器时很有用，例如设置为 onebot。
    /// </summary>
    public string? PlatformOverride { get; set; } = null;
}