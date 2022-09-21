namespace Flandre.Core.Messaging.Segments;

public class QuoteSegment : PrefixSegment
{
    public Message QuotedMessage { get; set; }

    public QuoteSegment(Message message)
    {
        QuotedMessage = message;
    }
}