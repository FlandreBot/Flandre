using Flandre.Core.Common;
using Flandre.Core.Messaging;

namespace Flandre.Core.Events.App;

/// <summary>
/// 指令调用事件
/// </summary>
public class AppCommandInvokingEvent : BaseEvent
{
    /// <summary>
    /// 即将执行的指令
    /// </summary>
    public Command Command { get; }

    /// <summary>
    /// 当前事件的消息上下文
    /// </summary>
    public MessageContext Context { get; }

    /// <summary>
    /// 解析后的指令
    /// </summary>
    public ParsedArgs ParsedArgs { get; }

    internal AppCommandInvokingEvent(Command command, MessageContext ctx, ParsedArgs args)
    {
        Command = command;
        Context = ctx;
        ParsedArgs = args;
    }
}