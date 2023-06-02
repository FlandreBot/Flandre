using Flandre.Core.Events;
using Flandre.Core.Messaging;
using Flandre.Framework.Common;

namespace Flandre.Framework.Events;

/// <summary>
/// 触发指令事件
/// </summary>
public class CommandInvokedEvent : FlandreEvent
{
    /// <summary>
    /// 将触发的指令
    /// </summary>
    public Command Command { get; }

    /// <summary>
    /// 当前消息
    /// </summary>
    public Message Message { get; }

    /// <summary>
    /// 触发失败后抛出的异常
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// 是否触发成功
    /// </summary>
    public bool IsSucceeded => Exception is null;

    /// <summary>
    /// 触发成功后将发送的消息
    /// </summary>
    public MessageContent? Response { get; }

    /// <summary>
    /// 用户 ID
    /// </summary>
    /// <remarks>等同于 <see cref="Message"/>.Sender.UserId</remarks>
    public string UserId => Message.Sender.UserId;

    /// <summary>
    /// 群组 ID
    /// </summary>
    /// <remarks>等同于 <see cref="Message"/>.GuildId</remarks>
    public string? GuildId => Message.GuildId;

    /// <summary>
    /// 频道 ID
    /// </summary>
    /// <remarks>等同于 <see cref="Message"/>.ChannelId</remarks>
    public string? ChannelId => Message.ChannelId;

    internal CommandInvokedEvent(Command command, Message message, Exception? exception, MessageContent? resp)
    {
        Command = command;
        Message = message;
        Exception = exception;
        Response = resp;
    }
}
