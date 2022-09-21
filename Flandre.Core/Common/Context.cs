using Flandre.Core.Messaging;

namespace Flandre.Core.Common;

public class Context
{
    public FlandreApp App { get; internal init; }

    public IBot Bot { get; internal init; }

    public Message Message { get; internal init; }

    public Context(FlandreApp app, IBot bot, Message message)
    {
        App = app;
        Bot = bot;
        Message = message;
    }
}