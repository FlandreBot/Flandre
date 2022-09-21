namespace Flandre.Core.Events.Bot;

public class BotFriendRequestedEvent
{
    public string RequesterName { get; }

    public string RequesterId { get; }

    public string Comment { get; }

    public BotFriendRequestedEvent(string requesterName, string requesterId, string comment)
    {
        RequesterName = requesterName;
        RequesterId = requesterId;
        Comment = comment;
    }
}