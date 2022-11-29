using Flandre.Core.Events;

namespace Flandre.Core.Common;

public abstract partial class Bot
{
    /// <summary>
    /// Bot 事件委托
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    public delegate void BotEventHandler<in TEvent>(Bot bot, TEvent e) where TEvent : BaseEvent;

    /// <summary>
    /// 日志记录
    /// </summary>
    public event BotEventHandler<BotLoggingEvent>? OnLogging;

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