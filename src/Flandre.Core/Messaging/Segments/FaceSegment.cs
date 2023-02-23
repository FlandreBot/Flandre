namespace Flandre.Core.Messaging.Segments;

/// <summary>
/// 表情消息段
/// </summary>
public class FaceSegment : InlineSegment
{
    /// <summary>
    /// 表情 ID
    /// </summary>
    public string FaceId { get; set; }

    /// <summary>
    /// 构造表情消息段
    /// </summary>
    /// <param name="faceId"></param>
    public FaceSegment(string faceId)
    {
        FaceId = faceId;
    }
}
