using Flandre.Core.Messaging;

namespace Flandre.Core.Events.Bot;

public class BotMessageReceivedEvent : BaseEvent
{
    public Message Message { get; }

    public BotMessageReceivedEvent(Message message)
    {
        Message = message;
    }
}