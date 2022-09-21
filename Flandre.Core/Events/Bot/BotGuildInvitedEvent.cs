namespace Flandre.Core.Events.Bot;

public class BotGuildInvitedEvent : BaseEvent
{
    public string GuildName { get; }

    public string GuildId { get; }

    public string InviterId { get; }

    public string InviterName { get; }

    public bool InviterIsAdmin { get; }

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