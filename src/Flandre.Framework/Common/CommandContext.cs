using Flandre.Core.Common;
using Flandre.Core.Messaging;
using Flandre.Framework.Interfaces;

namespace Flandre.Framework.Common;

public class CommandContext : MessageContext, IAppContext
{
    public FlandreApp App { get; }

    public CommandContext(FlandreApp app, Bot bot, Message message)
        : base(bot, message)
    {
        App = app;
    }
}