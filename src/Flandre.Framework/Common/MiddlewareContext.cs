using Flandre.Core.Common;
using Flandre.Core.Messaging;

namespace Flandre.Framework.Common;

public class MiddlewareContext : MessageContext
{
    public FlandreApp App { get; }

    public MessageContent? Response { get; set; }

    internal MiddlewareContext(FlandreApp app, Bot bot, Message message, MessageContent? resp)
        : base(bot, message)
    {
        App = app;
        Response = resp;
    }
}