using Flandre.Core.Common;
using Flandre.Core.Events;
using Flandre.Core.Messaging;
using Flandre.Core.Models;
using Konata.Core.Common;
using Konata.Core.Events;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces;
using Konata.Core.Interfaces.Api;
using BotConfig = Konata.Core.Common.BotConfig;
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

    /// <inheritdoc />
    public override string SelfId { get; }

    /// <summary>
    /// Konata 内部 bot
    /// </summary>
    public KonataInternalBot Internal { get; }

    private readonly KonataBotConfig _config;

    /// <inheritdoc />
    public override event BotEventHandler<BotMessageReceivedEvent>? OnMessageReceived;

    /// <inheritdoc />
    public override event BotEventHandler<BotGuildInvitedEvent>? OnGuildInvited;

    /// <inheritdoc />
    public override event BotEventHandler<BotGuildJoinRequestedEvent>? OnGuildJoinRequested;

    /// <inheritdoc />
    public override event BotEventHandler<BotFriendRequestedEvent>? OnFriendRequested;

    internal KonataBot(KonataBotConfig config)
    {
        Internal = BotFather.Create(config.Konata, config.Device, config.KeyStore);
        _config = config;

        SelfId = string.IsNullOrWhiteSpace(_config.SelfId)
            ? Internal.Uin.ToString()
            : _config.SelfId;

        Internal.OnFriendMessage += InternalOnFriendMessage;
        Internal.OnGroupMessage += InternalOnGroupMessage;
        Internal.OnGroupInvite += InternalOnGroupInvite;
        Internal.OnGroupRequestJoin += InternalOnGroupRequestJoin;
        Internal.OnFriendRequest += InternalOnFriendRequest;

        Internal.OnCaptcha += InternalOnCaptcha;
        Internal.OnLog += InternalOnLog;
    }

    #region 生命周期

    /// <inheritdoc />
    public override async Task Start()
    {
        Log(BotLogLevel.Debug, $"Starting bot {_config.SelfId}...");
        if (!await Internal.Login())
        {
            Log(BotLogLevel.Warning, $"Bot {_config.SelfId} login failed.");
            return;
        }

        _config.KeyStore = Internal.KeyStore;
        Log(BotLogLevel.Information, $"Bot {_config.SelfId} started.");
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

    private void InternalOnFriendMessage(KonataInternalBot bot, FriendMessageEvent e)
    {
        OnMessageReceived?.Invoke(this, new BotMessageReceivedEvent(e.Message.ToFlandreMessage(Platform)));
    }

    private void InternalOnGroupMessage(KonataInternalBot bot, GroupMessageEvent e)
    {
        OnMessageReceived?.Invoke(this, new BotMessageReceivedEvent(e.Message.ToFlandreMessage(Platform)));
    }

    private void InternalOnGroupInvite(KonataInternalBot bot, GroupInviteEvent e)
    {
        OnGuildInvited?.Invoke(this, new BotGuildInvitedEvent(
            e.GroupName, e.GroupUin.ToString(),
            e.InviterNick, e.InviterUin.ToString(), e.InviterIsAdmin) { EventMessage = e.Token });
    }

    private void InternalOnGroupRequestJoin(KonataInternalBot bot, GroupRequestJoinEvent e)
    {
        OnGuildJoinRequested?.Invoke(this, new BotGuildJoinRequestedEvent(
            e.GroupName, e.GroupUin.ToString(),
            e.ReqNick, e.ReqUin.ToString(), e.ReqComment) { EventMessage = e.Token });
    }

    private void InternalOnFriendRequest(KonataInternalBot bot, FriendRequestEvent e)
    {
        OnFriendRequested?.Invoke(this, new BotFriendRequestedEvent(
            e.ReqNick, e.ReqUin.ToString(), e.ReqComment) { EventMessage = e.Token });
    }

    private void InternalOnCaptcha(KonataInternalBot bot, CaptchaEvent e)
    {
        Log(BotLogLevel.Warning, $"Bot {_config.SelfId} needs login verification.");

        switch (e.Type)
        {
            case CaptchaEvent.CaptchaType.Sms:
                Log(BotLogLevel.Warning,
                    $"The phone verify code has been sent to {e.Phone}. Please input the code and press Enter.");
                Console.Write("Phone verify code: ");
                Internal.SubmitSmsCode(Console.ReadLine());
                break;

            case CaptchaEvent.CaptchaType.Slider:
                Log(BotLogLevel.Warning,
                    $"The Slider Captcha URL is: {e.SliderUrl}. Please input the ticket and press Enter.");
                Console.Write("Ticket: ");
                Internal.SubmitSliderTicket(Console.ReadLine());
                break;
        }
    }

    private void InternalOnLog(KonataInternalBot bot, LogEvent e)
    {
        switch (e.Level)
        {
            case LogLevel.Verbose:
                Log(BotLogLevel.Trace, e.EventMessage);
                break;

            case LogLevel.Information:
                Log(BotLogLevel.Information, e.EventMessage);
                break;

            case LogLevel.Warning:
                Log(BotLogLevel.Warning, e.EventMessage);
                break;

            case LogLevel.Exception:
                Log(BotLogLevel.Error, e.EventMessage);
                break;

            case LogLevel.Fatal:
                Log(BotLogLevel.Critical, e.EventMessage);
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
