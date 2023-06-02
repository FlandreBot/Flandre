namespace Flandre.Core.Events;

/// <summary>
/// 入群申请事件
/// </summary>
public class BotGuildJoinRequestedEvent : FlandreEvent
{
    /// <summary>
    /// Guild 名称
    /// </summary>
    public string GuildName { get; }

    /// <summary>
    /// Guild ID
    /// </summary>
    public string GuildId { get; }

    /// <summary>
    /// 申请人名称
    /// </summary>
    public string RequesterName { get; }

    /// <summary>
    /// 申请人 ID
    /// </summary>
    public string RequesterId { get; }

    /// <summary>
    /// 申请备注
    /// </summary>
    public string Comment { get; }

    /// <summary>
    /// 构造事件
    /// </summary>
    /// <param name="guildName">Guild 名称</param>
    /// <param name="guildId">Guild ID</param>
    /// <param name="requesterName">申请人名称</param>
    /// <param name="requesterId">申请人 ID</param>
    /// <param name="comment">申请备注</param>
    public BotGuildJoinRequestedEvent(string guildName, string guildId, string requesterName, string requesterId,
        string comment)
    {
        GuildName = guildName;
        GuildId = guildId;
        RequesterName = requesterName;
        RequesterId = requesterId;
        Comment = comment;
    }
}
