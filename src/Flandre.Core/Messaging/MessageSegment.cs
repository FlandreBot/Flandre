namespace Flandre.Core.Messaging;

/// <summary>
/// 消息段基类
/// </summary>
public abstract class MessageSegment
{
}

/// <summary>
/// 内联消息段
/// </summary>
public abstract class InlineSegment : MessageSegment
{
}

/// <summary>
/// 资源消息段
/// </summary>
public abstract class ResourceSegment : MessageSegment
{
    /// <summary>
    /// 资源数据
    /// </summary>
    public byte[]? Data { get; set; }

    /// <summary>
    /// 资源文件路径
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// 资源 URL
    /// </summary>
    public string? Url { get; set; }
}

/// <summary>
/// 前缀消息段
/// </summary>
public abstract class PrefixSegment : MessageSegment
{
}