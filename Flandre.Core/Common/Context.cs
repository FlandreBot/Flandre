using Flandre.Core.Messaging;

namespace Flandre.Core.Common;

public class Context
{
    public FlandreApp App { get; init; }

    public IBot Bot { get; init; }

    public Context(FlandreApp app, IBot bot)
    {
        App = app;
        Bot = bot;
    }
}

public class MessageContext : Context
{
    public Message Message { get; init; }
    
    public MessageContext(FlandreApp app, IBot bot, Message message)
        : base(app, bot)
    {
        Message = message;
    }
}