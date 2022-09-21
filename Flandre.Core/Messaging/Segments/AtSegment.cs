namespace Flandre.Core.Messaging.Segments;

public class AtSegment : InlineSegment
{
    public string? UserId { get; set; }

    public AtSegmentScope Scope { get; set; } = AtSegmentScope.Single;

    public AtSegment(string userId)
    {
        UserId = userId;
    }

    public AtSegment(AtSegmentScope scope)
    {
        Scope = scope;
    }

    public static AtSegment AtAll()
    {
        return new(AtSegmentScope.All);
    }
}

public enum AtSegmentScope
{
    Single,
    All
}