using Flandre.Core.Messaging.Segments;

namespace Flandre.Core.Messaging;

public class MessageBuilder
{
    public List<MessageSegment> Segments = new();

    public MessageBuilder Text(string text)
    {
        Segments.Add(new TextSegment(text));
        return this;
    }

    public MessageBuilder Image(byte[] data, string? type = null)
    {
        Segments.Add(ImageSegment.FromData(data, type));
        return this;
    }

    public MessageBuilder Image(ImageSegment imageSegment)
    {
        Segments.Add(imageSegment);
        return this;
    }

    public MessageBuilder Add(MessageSegment segment)
    {
        Segments.Add(segment);
        return this;
    }

    public MessageContent Build()
    {
        return new MessageContent(Segments);
    }
}