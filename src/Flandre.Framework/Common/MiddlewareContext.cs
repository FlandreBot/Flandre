using Flandre.Core.Common;
using Flandre.Core.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Flandre.Framework.Common;

public sealed class MiddlewareContext : MessageContext
{
    internal readonly IServiceScope ServiceScope;

    /// <summary>
    /// App 中的服务为全局服务。若要使用 Scoped 服务，请换用 MiddlewareContext.Services
    /// </summary>
    public FlandreApp App { get; }

    public IServiceProvider Services => ServiceScope.ServiceProvider;

    public MessageContent? Response { get; set; }

    internal MiddlewareContext(FlandreApp app, Bot bot, Message message, MessageContent? resp)
        : base(bot, message)
    {
        App = app;
        ServiceScope = app.Services.CreateScope();
        Response = resp;
    }
}