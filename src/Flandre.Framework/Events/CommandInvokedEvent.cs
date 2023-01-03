using Flandre.Core.Events;
using Flandre.Core.Messaging;
using Flandre.Framework.Common;

namespace Flandre.Framework.Events;

public class CommandInvokedEvent : BaseEvent
{
    public Command Command { get; }

    /// <summary>
    /// 当前消息
    /// </summary>
    public Message Message { get; }

    public Exception? Exception { get; }

    public bool IsSucceed => Exception is null;

    public MessageContent? Response { get; }

    /// <summary>
    /// 用户 ID，等同于 Message.Sender.UserId
    /// </summary>
    public string UserId => Message.Sender.UserId;

    /// <summary>
    /// 群组 ID，等同于 Message.GuildId
    /// </summary>
    public string? GuildId => Message.GuildId;

    /// <summary>
    /// 频道 ID，等同于 Message.ChannelId
    /// </summary>
    public string? ChannelId => Message.ChannelId;

    internal CommandInvokedEvent(Command command, Message message, Exception? exception, MessageContent? resp)
    {
        Command = command;
        Message = message;
        Exception = exception;
        Response = resp;
    }
}