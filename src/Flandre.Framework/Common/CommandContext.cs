using Flandre.Core.Common;
using Flandre.Core.Messaging;

namespace Flandre.Framework.Common;

public class CommandContext : MessageContext
{
    public FlandreApp App { get; }

    public CommandContext(FlandreApp app, Bot bot, Message message)
        : base(bot, message)
    {
        App = app;
    }
}