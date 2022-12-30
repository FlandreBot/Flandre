using Flandre.Core.Events;
using Flandre.Framework.Common;

namespace Flandre.Framework.Events;

public class CommandInvokingEvent : BaseEvent
{
    public Command Command { get; }

    internal CommandInvokingEvent(Command command)
    {
        Command = command;
    }
}