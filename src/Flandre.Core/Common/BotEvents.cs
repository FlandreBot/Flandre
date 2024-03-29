﻿using Flandre.Core.Events;

namespace Flandre.Core.Common;

/// <summary>
/// Bot 事件委托
/// </summary>
/// <typeparam name="TEvent">事件类型</typeparam>
public delegate void BotEventHandler<in TEvent>(Bot bot, TEvent e) where TEvent : FlandreEvent;

public abstract partial class Bot
{
    /// <summary>
    /// 日志记录
    /// </summary>
    public event BotEventHandler<BotLoggingEvent>? Logging;

    /// <summary>
    /// 收到消息
    /// </summary>
    public abstract event BotEventHandler<BotMessageReceivedEvent>? MessageReceived;

    /// <summary>
    /// 收到群组邀请
    /// </summary>
    public abstract event BotEventHandler<BotGuildInvitedEvent>? GuildInvited;

    /// <summary>
    /// 收到加群申请
    /// </summary>
    public abstract event BotEventHandler<BotGuildJoinRequestedEvent>? GuildJoinRequested;

    /// <summary>
    /// 收到好友申请
    /// </summary>
    public abstract event BotEventHandler<BotFriendRequestedEvent>? FriendRequested;

    /// <summary>
    /// 处理拉群邀请
    /// </summary>
    /// <param name="e">拉群邀请事件</param>
    /// <param name="approve">是否同意</param>
    /// <param name="comment">附加说明</param>
    public virtual Task HandleGuildInvitationAsync(BotGuildInvitedEvent e, bool approve, string? comment = null)
        => LogNotSupportedAsync(nameof(HandleGuildInvitationAsync));

    /// <summary>
    /// 处理加群申请
    /// </summary>
    /// <param name="e">加群申请事件</param>
    /// <param name="approve">是否同意</param>
    /// <param name="comment">附加说明</param>
    public virtual Task HandleGuildJoinRequestAsync(BotGuildJoinRequestedEvent e, bool approve, string? comment = null)
        => LogNotSupportedAsync(nameof(HandleGuildJoinRequestAsync));

    /// <summary>
    /// 处理好友申请
    /// </summary>
    /// <param name="e">好友申请事件</param>
    /// <param name="approve">是否同意</param>
    /// <param name="comment">附加说明</param>
    public virtual Task HandleFriendRequestAsync(BotFriendRequestedEvent e, bool approve, string? comment = null)
        => LogNotSupportedAsync(nameof(HandleFriendRequestAsync));
}
