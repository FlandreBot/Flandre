namespace Flandre.Core.Messaging.Segments;

/// <summary>
/// 引用消息段（如回复）
/// </summary>
public class QuoteSegment : PrefixSegment
{
    /// <summary>
    /// 引用的消息
    /// </summary>
    public Message QuotedMessage { get; set; }

    /// <summary>
    /// 构造引用消息段
    /// </summary>
    /// <param name="message">引用的消息</param>
    public QuoteSegment(Message message)
    {
        QuotedMessage = message;
    }
}