using Flandre.Core.Messaging.Segments;

namespace Flandre.Adapters.OneBot.Segments;

public class OneBotImageSegment : ImageSegment
{
    public string? Filename { get; set; }

    public string? SubType { get; set; }

    public int? Id { get; set; }
}
