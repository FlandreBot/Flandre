namespace Flandre.Core.Events.Bot;

public class BotGuildRequestedEvent : BaseEvent
{
    public string GuildName { get; }

    public string GuildId { get; }

    public string RequesterName { get; }

    public string RequesterId { get; }

    public string Comment { get; }

    public BotGuildRequestedEvent(string guildName, string guildId, string requesterName, string requesterId,
        string comment)
    {
        GuildName = guildName;
        GuildId = guildId;
        RequesterName = requesterName;
        RequesterId = requesterId;
        Comment = comment;
    }
}