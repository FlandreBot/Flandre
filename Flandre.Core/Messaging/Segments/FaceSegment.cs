namespace Flandre.Core.Messaging.Segments;

public class FaceSegment : InlineSegment
{
    public string FaceId { get; set; }

    public FaceSegment(string faceId)
    {
        FaceId = faceId;
    }
}