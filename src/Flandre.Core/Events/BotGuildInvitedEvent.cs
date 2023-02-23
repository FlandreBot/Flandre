namespace Flandre.Core.Events;

/// <summary>
/// bot 收到 Guild 邀请事件
/// </summary>
public class BotGuildInvitedEvent : BaseEvent
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
    /// 邀请人 ID
    /// </summary>
    public string InviterId { get; }

    /// <summary>
    /// 邀请人名称
    /// </summary>
    public string InviterName { get; }

    /// <summary>
    /// 邀请人是否为管理员。如果适配器不支持应始终返回 true。
    /// </summary>
    public bool InviterIsAdmin { get; }

    /// <summary>
    /// 构造事件
    /// </summary>
    /// <param name="guildName">Guild 名称</param>
    /// <param name="guildId">Guild ID</param>
    /// <param name="inviterName">邀请人名称</param>
    /// <param name="inviterId">邀请人 ID</param>
    /// <param name="inviterIsAdmin">邀请人是否为管理员</param>
    public BotGuildInvitedEvent(string guildName, string guildId,
        string inviterName, string inviterId, bool inviterIsAdmin)
    {
        GuildName = guildName;
        GuildId = guildId;
        InviterName = inviterName;
        InviterId = inviterId;
        InviterIsAdmin = inviterIsAdmin;
    }
}
