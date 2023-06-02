using Flandre.Core.Events;
using Flandre.Core.Messaging;
using Flandre.Framework.Common;

namespace Flandre.Framework.Events;

public class CommandInvokingEvent : FlandreEvent
{
    public Command Command { get; }

    /// <summary>
    /// 当前消息
    /// </summary>
    public Message Message { get; }

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

    internal bool IsCancelled { get; private set; }

    internal CommandInvokingEvent(Command command, Message message)
    {
        Command = command;
        Message = message;
    }

    public void Cancel() => IsCancelled = true;
}
