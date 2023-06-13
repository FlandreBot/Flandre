using Flandre.Core.Common;
using Flandre.Core.Messaging;
using Flandre.Core.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Flandre.Framework.Common;

/// <summary>
/// 中间件上下文，包含当前时刻所需的全部对象
/// </summary>
public sealed class MiddlewareContext : MessageContext
{
    internal readonly IServiceScope ServiceScope;

    /// <summary>
    /// 所在 <see cref="FlandreApp"/> 实例，包含全局服务
    /// </summary>
    public FlandreApp App { get; }

    /// <summary>
    /// 域内服务
    /// </summary>
    public IServiceProvider Services => ServiceScope.ServiceProvider;

    /// <summary>
    /// 即将发送的回复
    /// </summary>
    public MessageContent? Response { get; set; }

    /// <summary>
    /// 当前所在的指令
    /// </summary>
    public Command? Command { get; internal set; }

    /// <summary>
    /// 执行指令产生的异常。如果指令成功执行，则该项为 null
    /// </summary>
    public Exception? Exception { get; internal set; }

    // TODO: 添加 IsFailed 和 FailReason 属性
    // TODO: enum MiddlewareFailReason { None, Exception, MissingArgument, ... }

    private IDictionary<string, object?>? _properties;

    /// <summary>
    /// 中间件属性，用于在中间件内传递消息
    /// </summary>
    public IDictionary<string, object?> Properties => _properties ??= new Dictionary<string, object?>();

    internal StringParser? CommandStringParser { get; set; }

    internal MiddlewareContext(FlandreApp app, Bot bot, Message message, MessageContent? resp)
        : base(bot, message)
    {
        App = app;
        ServiceScope = app.Services.CreateScope();
        Response = resp;
    }
}
