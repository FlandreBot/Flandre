namespace Flandre.Core.Messaging;

public abstract class MessageSegment
{
}

public abstract class InlineSegment : MessageSegment
{
}

public abstract class ResourceSegment : MessageSegment
{
    /// <summary>
    ///     资源数据
    /// </summary>
    public byte[]? Data { get; set; }

    /// <summary>
    ///     资源文件路径
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    ///     资源 URL
    /// </summary>
    public string? Url { get; set; }
}

public abstract class PrefixSegment : MessageSegment
{
}

public enum MessageSegmentType
{
    // Inline segments
    Text,
    At,
    Face,

    // Resource segments
    Image,
    Audio,

    // Prefix segments
    Quote
}