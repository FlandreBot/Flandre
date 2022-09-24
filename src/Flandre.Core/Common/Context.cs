using Flandre.Core.Messaging;

namespace Flandre.Core.Common;

/// <summary>
/// 基础上下文
/// </summary>
public class Context
{
    /// <summary>
    /// FlandreApp 实例
    /// </summary>
    public FlandreApp App { get; init; }

    /// <summary>
    /// 当前 bot 实例
    /// </summary>
    public IBot Bot { get; init; }

    /// <summary>
    /// 构造上下文
    /// </summary>
    /// <param name="app">FlandreApp 实例</param>
    /// <param name="bot">bot 实例</param>
    public Context(FlandreApp app, IBot bot)
    {
        App = app;
        Bot = bot;
    }
}

/// <summary>
/// 消息上下文
/// </summary>
public class MessageContext : Context
{
    /// <summary>
    /// 当前消息
    /// </summary>
    public Message Message { get; init; }

    /// <summary>
    /// 构造消息上下文
    /// </summary>
    /// <param name="app">FlandreApp 实例</param>
    /// <param name="bot">bot 实例</param>
    /// <param name="message">消息</param>
    public MessageContext(FlandreApp app, IBot bot, Message message)
        : base(app, bot)
    {
        Message = message;
    }
}