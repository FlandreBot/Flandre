using Flandre.Core.Messaging.Segments;

namespace Flandre.Adapters.OneBot.Segments;

public class OneBotRecordSegment : AudioSegment
{
    public string? Filename { get; set; }

    public int? Magic { get; set; }
}