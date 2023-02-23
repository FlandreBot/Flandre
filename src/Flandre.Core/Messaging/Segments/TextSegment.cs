namespace Flandre.Core.Messaging.Segments;

/// <summary>
/// 文本消息段
/// </summary>
public class TextSegment : InlineSegment
{
    /// <summary>
    /// 文本
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// 构造文本消息段
    /// </summary>
    /// <param name="text">文本</param>
    public TextSegment(string text)
    {
        Text = text;
    }
}
