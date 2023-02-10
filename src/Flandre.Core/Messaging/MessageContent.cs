using System.Collections;
using Flandre.Core.Messaging.Segments;

namespace Flandre.Core.Messaging;

/// <summary>
/// 消息内容
/// </summary>
public class MessageContent : IEnumerable<MessageSegment>
{
    /// <summary>
    /// 消息内容包含的消息段
    /// </summary>
    public IEnumerable<MessageSegment> Segments { get; }

    /// <summary>
    /// 使用消息段构造消息内容
    /// </summary>
    /// <param name="segments"></param>
    public MessageContent(IEnumerable<MessageSegment> segments)
    {
        Segments = segments;
    }

    /// <summary>
    /// 获取类型匹配的消息段
    /// </summary>
    /// <typeparam name="TSegment">消息段类型</typeparam>
    public TSegment? GetSegment<TSegment>() where TSegment : MessageSegment
    {
        return (TSegment?)Segments.FirstOrDefault(segment => segment is TSegment);
    }

    /// <summary>
    /// 获取所有类型匹配的消息段
    /// </summary>
    /// <typeparam name="TSegment">消息段类型</typeparam>
    public IEnumerable<TSegment> GetSegments<TSegment>() where TSegment : MessageSegment
    {
        return Segments.Where(segment => segment is TSegment).Cast<TSegment>();
    }

    /// <summary>
    /// 获取消息内容中的所有文本
    /// </summary>
    public string GetText()
    {
        return string.Join("", GetSegments<TextSegment>().Select(s => s.Text));
    }

    /// <summary>
    /// 由 Message 隐式转换
    /// </summary>
    public static implicit operator MessageContent(Message message)
    {
        return message.Content;
    }

    /// <summary>
    /// 由 MessageBuilder 隐式转换
    /// </summary>
    public static implicit operator MessageContent(MessageBuilder builder)
    {
        return builder.Build();
    }

    /// <summary>
    /// 由消息段隐式转换
    /// </summary>
    public static implicit operator MessageContent(MessageSegment segment)
    {
        return new MessageContent(new[] { segment });
    }

    /// <summary>
    /// 由字符串隐式转换
    /// </summary>
    public static implicit operator MessageContent(string text)
    {
        return new MessageContent(new[] { new TextSegment(text) });
    }

    /// <summary>
    /// 获取 Enumerator
    /// </summary>
    /// <returns></returns>
    public IEnumerator<MessageSegment> GetEnumerator()
    {
        return Segments.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}