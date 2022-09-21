namespace Flandre.Core.Messaging.Segments;

public class TextSegment : InlineSegment
{
    public string Text { get; set; }

    public TextSegment(string text)
    {
        Text = text;
    }
}