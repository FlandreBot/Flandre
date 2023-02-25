using Flandre.Core.Common;
using Flandre.Core.Messaging;

namespace Flandre.Framework.Common;

/// <summary>
/// 指令上下文
/// </summary>
public class CommandContext : MessageContext
{
    /// <summary>
    /// 所在的 <see cref="FlandreApp"/> 实例
    /// </summary>
    public FlandreApp App { get; }

    /// <summary>
    /// 构造实例
    /// </summary>
    /// <param name="app"></param>
    /// <param name="bot"></param>
    /// <param name="message"></param>
    public CommandContext(FlandreApp app, Bot bot, Message message)
        : base(bot, message)
    {
        App = app;
    }
}
