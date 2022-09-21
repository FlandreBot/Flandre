using System.Collections;
using Flandre.Core.Messaging.Segments;

namespace Flandre.Core.Messaging;

public class MessageContent : IEnumerable<MessageSegment>
{
    private readonly IEnumerable<MessageSegment> _segments;

    public MessageContent(IEnumerable<MessageSegment> segments)
    {
        _segments = segments;
    }

    public TSegment? GetSegment<TSegment>() where TSegment : MessageSegment
    {
        return (TSegment?)_segments.FirstOrDefault(segment => segment is TSegment);
    }

    public IEnumerable<TSegment> GetSegments<TSegment>() where TSegment : MessageSegment
    {
        return _segments.Where(segment => segment is TSegment).Cast<TSegment>();
    }

    public string GetText()
    {
        return string.Join("", GetSegments<TextSegment>().Select(s => s.Text));
    }

    /// <summary>
    ///     由 Message 隐式转换
    /// </summary>
    public static implicit operator MessageContent(Message message)
    {
        return message.Content;
    }

    /// <summary>
    ///     由 MessageBuilder 隐式转换
    /// </summary>
    public static implicit operator MessageContent(MessageBuilder builder)
    {
        return builder.Build();
    }

    /// <summary>
    ///     由字符串隐式转换
    /// </summary>
    public static implicit operator MessageContent(string text)
    {
        return new(new[] { new TextSegment(text) });
    }

    public IEnumerator<MessageSegment> GetEnumerator()
    {
        return _segments.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}