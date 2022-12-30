using Flandre.Core.Events;
using Flandre.Framework.Common;

namespace Flandre.Framework.Events;

public class CommandInvokedEvent : BaseEvent
{
    public Command Command { get; }

    public Exception? Exception { get; }

    public bool IsSucceed => Exception is null;

    internal CommandInvokedEvent(Command command, Exception? exception)
    {
        Command = command;
        Exception = exception;
    }
}