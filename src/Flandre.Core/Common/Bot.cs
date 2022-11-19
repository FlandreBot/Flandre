using Flandre.Core.Events.Bot;
using Flandre.Core.Messaging;
using Flandre.Core.Models;
using Flandre.Core.Utils;

namespace Flandre.Core.Common;

/// <summary>
/// 机器人
/// </summary>
public abstract class Bot
{
    /// <summary>
    /// Bot 所在平台名称
    /// </summary>
    public abstract string Platform { get; }

    /// <summary>
    /// Bot 自身 ID
    /// </summary>
    public abstract string SelfId { get; }

    /// <summary>
    /// 获取 Logger
    /// </summary>
    protected abstract Logger GetLogger();

    /// <summary>
    /// 启动 Bot 实例
    /// </summary>
    public virtual Task Start() => Task.CompletedTask;

    /// <summary>
    /// 停止 Bot 实例
    /// </summary>
    public virtual Task Stop() => Task.CompletedTask;

    /// <summary>
    /// 该方法不受支持，发出警告
    /// </summary>
    /// <param name="method">方法名称</param>
    /// <returns>Task.CompletedTask</returns>
    protected Task LogNotSupported(string method)
    {
        GetLogger().Warning($"{Platform} 平台暂不支持 {method} 方法。");
        return Task.CompletedTask;
    }

    /// <summary>
    /// 该方法不受支持，发出警告
    /// </summary>
    /// <param name="method">方法名称</param>
    /// <param name="result">返回值</param>
    /// <returns>Task.FromResult&lt;TResult&gt;(result)</returns>
    protected Task<TResult> LogNotSupported<TResult>(string method, TResult result)
    {
        GetLogger().Warning($"{Platform} 平台暂不支持 {method} 方法。");
        return Task.FromResult(result);
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="sourceType">消息类型</param>
    /// <param name="channelId">频道 ID</param>
    /// <param name="userId">用户 ID</param>
    /// <param name="content">消息内容</param>
    /// <param name="guildId">群组 ID</param>
    public virtual Task<string?> SendMessage(MessageSourceType sourceType, string? channelId, string? userId,
        MessageContent content,
        string? guildId = null)
    {
        return sourceType switch
        {
            MessageSourceType.Channel => SendChannelMessage(channelId!, content, guildId),
            MessageSourceType.Private => SendPrivateMessage(userId!, content),
            _ => Task.FromResult<string?>(null)
        };
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="message">消息对象</param>
    /// <param name="contentOverride">覆盖消息对象的内容，可选</param>
    public virtual Task<string?> SendMessage(Message message, MessageContent? contentOverride = null)
    {
        return SendMessage(message.SourceType, message.ChannelId, message.Sender.UserId,
            contentOverride ?? message.Content, message.GuildId);
    }

    /// <summary>
    /// 发送频道 (Channel) 消息
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="content">消息内容</param>
    /// <param name="guildId">群组 ID</param>
    public virtual Task<string?> SendChannelMessage(string channelId, MessageContent content, string? guildId = null)
        => LogNotSupported<string?>(nameof(SendChannelMessage), null);

    /// <summary>
    /// 发送私聊消息
    /// </summary>
    /// <param name="userId">用户 ID</param>
    /// <param name="content">消息内容</param>
    public virtual Task<string?> SendPrivateMessage(string userId, MessageContent content)
        => LogNotSupported<string?>(nameof(SendPrivateMessage), null);

    /// <summary>
    /// 删除（撤回）消息
    /// </summary>
    /// <param name="messageId">消息 ID</param>
    public virtual Task DeleteMessage(string messageId)
        => LogNotSupported(nameof(DeleteMessage));

    /// <summary>
    /// 获取自身信息
    /// </summary>
    public virtual Task<User?> GetSelf()
        => LogNotSupported<User?>(nameof(GetSelf), null);

    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <param name="userId">用户 ID</param>
    /// <param name="guildId">群组 ID</param>
    public virtual Task<User?> GetUser(string userId, string? guildId = null)
        => LogNotSupported<User?>(nameof(GetUser), null);

    /// <summary>
    /// 获取好友列表
    /// </summary>
    public virtual Task<IEnumerable<User>> GetFriendList()
        => LogNotSupported<IEnumerable<User>>(nameof(GetFriendList), Array.Empty<User>());

    /// <summary>
    /// 获取群组信息
    /// </summary>
    /// <param name="guildId">群组 ID</param>
    public virtual Task<Guild?> GetGuild(string guildId)
        => LogNotSupported<Guild?>(nameof(GetGuild), null);

    /// <summary>
    /// 获取群组列表
    /// </summary>
    public virtual Task<IEnumerable<Guild>> GetGuildList()
        => LogNotSupported<IEnumerable<Guild>>(nameof(GetGuildList), Array.Empty<Guild>());

    /// <summary>
    /// 获取群组成员信息
    /// </summary>
    /// <param name="guildId">群组 ID</param>
    /// <param name="userId">用户 ID</param>
    public virtual Task<GuildMember?> GetGuildMember(string guildId, string userId)
        => LogNotSupported<GuildMember?>(nameof(GetGuildMember), null);

    /// <summary>
    /// 获取群组成员列表
    /// </summary>
    /// <param name="guildId">群组 ID</param>
    public virtual Task<IEnumerable<GuildMember>> GetGuildMemberList(string guildId)
        => LogNotSupported<IEnumerable<GuildMember>>(nameof(GetGuildList), Array.Empty<GuildMember>());

    /// <summary>
    /// 获取频道信息
    /// </summary>
    /// <param name="channelId">频道 ID</param>
    /// <param name="guildId">群组 ID</param>
    public virtual Task<Channel?> GetChannel(string channelId, string? guildId = null)
        => LogNotSupported<Channel?>(nameof(GetChannel), null);

    /// <summary>
    /// 获取频道列表
    /// </summary>
    /// <param name="guildId">群组 ID</param>
    public virtual Task<IEnumerable<Channel>> GetChannelList(string guildId)
        => LogNotSupported<IEnumerable<Channel>>(nameof(GetGuildList), Array.Empty<Channel>());

    /// <summary>
    /// Bot 事件委托
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    public delegate void BotEventHandler<in TEvent>(Bot bot, TEvent e);

    /// <summary>
    /// 收到消息
    /// </summary>
    public abstract event BotEventHandler<BotMessageReceivedEvent>? OnMessageReceived;

    /// <summary>
    /// 收到群组邀请
    /// </summary>
    public abstract event BotEventHandler<BotGuildInvitedEvent>? OnGuildInvited;

    /// <summary>
    /// 收到加群申请
    /// </summary>
    public abstract event BotEventHandler<BotGuildJoinRequestedEvent>? OnGuildJoinRequested;

    /// <summary>
    /// 收到好友申请
    /// </summary>
    public abstract event BotEventHandler<BotFriendRequestedEvent>? OnFriendRequested;

    /// <summary>
    /// 处理拉群邀请
    /// </summary>
    /// <param name="e">拉群邀请事件</param>
    /// <param name="approve">是否同意</param>
    /// <param name="comment">附加说明</param>
    public virtual Task HandleGuildInvitation(BotGuildInvitedEvent e, bool approve, string? comment = null)
        => LogNotSupported(nameof(HandleGuildInvitation));

    /// <summary>
    /// 处理加群申请
    /// </summary>
    /// <param name="e">加群申请事件</param>
    /// <param name="approve">是否同意</param>
    /// <param name="comment">附加说明</param>
    public virtual Task HandleGuildJoinRequest(BotGuildJoinRequestedEvent e, bool approve, string? comment = null)
        => LogNotSupported(nameof(HandleGuildJoinRequest));

    /// <summary>
    /// 处理好友申请
    /// </summary>
    /// <param name="e">好友申请事件</param>
    /// <param name="approve">是否同意</param>
    /// <param name="comment">附加说明</param>
    public virtual Task HandleFriendRequest(BotFriendRequestedEvent e, bool approve, string? comment = null)
        => LogNotSupported(nameof(HandleFriendRequest));
}

/// <summary>
/// Bot 基本配置
/// </summary>
public class BotConfig
{
    /// <summary>
    /// 自身 ID
    /// </summary>
    public string SelfId { get; set; } = "";
}