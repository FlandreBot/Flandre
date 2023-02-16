namespace Flandre.Framework.Common;

public sealed class CommandInvokeException : Exception
{
    public CommandInvokeException(string message) : base(message)
    {
    }
}