namespace Flandre.Framework.Common;

/// <summary>
/// 指令调用异常
/// </summary>
public sealed class CommandInvokeException : Exception
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public CommandInvokeException(string message) : base(message)
    {
    }
}
