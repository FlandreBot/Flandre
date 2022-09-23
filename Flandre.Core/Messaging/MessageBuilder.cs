using Flandre.Core.Messaging.Segments;

namespace Flandre.Core.Messaging;

/// <summary>
///     消息构造器
/// </summary>
public class MessageBuilder
{
    public List<MessageSegment> Segments = new();

    /// <summary>
    ///     添加文本消息段
    /// </summary>
    /// <param name="text">文本</param>
    public MessageBuilder Text(string text)
    {
        Segments.Add(new TextSegment(text));
        return this;
    }

    /// <summary>
    ///     添加图片消息段
    /// </summary>
    /// <param name="imageSegment">图片消息段</param>
    public MessageBuilder Image(ImageSegment imageSegment)
    {
        Segments.Add(imageSegment);
        return this;
    }

    /// <summary>
    ///     添加消息段
    /// </summary>
    /// <param name="segment">消息段</param>
    public MessageBuilder Add(MessageSegment segment)
    {
        Segments.Add(segment);
        return this;
    }

    /// <summary>
    ///     构造为 MessageContent
    /// </summary>
    public MessageContent Build()
    {
        return new MessageContent(Segments);
    }
}