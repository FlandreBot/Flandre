using Flandre.Core.Messaging;

namespace Flandre.Core.Events;

/// <summary>
/// 消息接收事件
/// </summary>
public class BotMessageReceivedEvent : BaseEvent
{
    /// <summary>
    /// 接收到的消息
    /// </summary>
    public Message Message { get; }

    /// <summary>
    /// 构造事件
    /// </summary>
    /// <param name="message">接收到的消息</param>
    public BotMessageReceivedEvent(Message message)
    {
        Message = message;
    }
}