using Flandre.Core.Common;
using Flandre.Core.Messaging;
using Flandre.Core.Utils;

namespace Flandre.Core.Events.App;

/// <summary>
/// 指令解析前
/// </summary>
public class AppCommandParsingEvent : CancellableEvent
{
    /// <summary>
    /// 当前事件的消息上下文
    /// </summary>
    public MessageContext Context { get; }

    /// <summary>
    /// 匹配到的指令
    /// </summary>
    public Command Command { get; }

    /// <summary>
    /// 指令解析使用的字符串解析器，包含待解析的参数和选项
    /// </summary>
    public StringParser Parser { get; }

    /// <summary>
    /// 自定义解析参数
    /// </summary>
    public ParsedArgs? CustomArgs { get; set; } = null;

    internal AppCommandParsingEvent(MessageContext ctx, Command command, StringParser parser)
    {
        Context = ctx;
        Command = command;
        Parser = parser;
    }
}