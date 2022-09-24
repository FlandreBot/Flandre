namespace Flandre.Core.Messaging.Segments;

/// <summary>
/// at 消息段
/// </summary>
public class AtSegment : InlineSegment
{
    /// <summary>
    /// 用户 ID
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// at 范围
    /// </summary>
    public AtSegmentScope Scope { get; set; } = AtSegmentScope.Single;

    /// <summary>
    /// 使用用户 ID 构造 at 消息段
    /// </summary>
    /// <param name="userId">用户 ID</param>
    public AtSegment(string userId)
    {
        UserId = userId;
    }

    /// <summary>
    /// 使用用户 ID 构造 at 消息段
    /// </summary>
    /// <param name="scope">at 范围</param>
    public AtSegment(AtSegmentScope scope)
    {
        Scope = scope;
    }

    /// <summary>
    /// 快捷 at 全体成员
    /// </summary>
    public static AtSegment AtAll()
    {
        return new AtSegment(AtSegmentScope.All);
    }
}

/// <summary>
/// at 范围
/// </summary>
public enum AtSegmentScope
{
    /// <summary>
    /// at 单个用户
    /// </summary>
    Single,

    /// <summary>
    /// at 全体成员
    /// </summary>
    All
}