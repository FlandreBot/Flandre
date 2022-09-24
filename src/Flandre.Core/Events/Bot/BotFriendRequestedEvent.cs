namespace Flandre.Core.Events.Bot;

/// <summary>
/// bot 好友申请事件
/// </summary>
public class BotFriendRequestedEvent : BaseEvent
{
    /// <summary>
    /// 申请人名称
    /// </summary>
    public string RequesterName { get; }

    /// <summary>
    /// 申请人 ID
    /// </summary>
    public string RequesterId { get; }

    /// <summary>
    /// 
    /// </summary>
    public string Comment { get; }

    /// <summary>
    /// 构造好友申请事件
    /// </summary>
    /// <param name="requesterName">申请人名称</param>
    /// <param name="requesterId">申请人 ID</param>
    /// <param name="comment">申请备注</param>
    public BotFriendRequestedEvent(string requesterName, string requesterId, string comment)
    {
        RequesterName = requesterName;
        RequesterId = requesterId;
        Comment = comment;
    }
}