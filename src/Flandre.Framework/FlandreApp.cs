using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Flandre.Core.Common;
using Flandre.Core.Messaging;
using Flandre.Framework.Common;
using Flandre.Framework.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Flandre.Framework;

public sealed partial class FlandreApp : IHost
{
    private readonly IHost _hostApp;
    private readonly IOptionsMonitor<FlandreAppOptions> _appOptions;
    private readonly List<IAdapter> _adapters;
    private readonly List<Type> _pluginTypes;
    private readonly List<Func<MiddlewareContext, Action, Task>> _middlewares = new();
    public List<Bot> Bots { get; } = new();

    public IServiceProvider Services => _hostApp.Services;
    public ILogger<FlandreApp> Logger { get; }

    #region 指令解析相关

    internal CommandNode RootCommandNode { get; } = new(true);
    internal Dictionary<string, Command> StringShortcuts { get; } = new();
    internal Dictionary<Regex, Command> RegexShortcuts { get; } = new();

    #endregion

    internal ConcurrentDictionary<string, string> GuildAssignees { get; } = new();
    internal ConcurrentDictionary<string, TaskCompletionSource<Message?>> CommandSessions { get; } = new();

    public static FlandreAppBuilder CreateBuilder(string[]? args = null) => new(args);
    public static FlandreAppBuilder CreateBuilder(HostApplicationBuilderSettings? settings) => new(settings);

    internal FlandreApp(IHost hostApp, List<Type> pluginTypes, List<IAdapter> adapters)
    {
        _hostApp = hostApp;
        _appOptions = _hostApp.Services.GetRequiredService<IOptionsMonitor<FlandreAppOptions>>();
        _pluginTypes = pluginTypes;
        _adapters = adapters;

        Logger = Services.GetRequiredService<ILogger<FlandreApp>>();

        foreach (var adapter in _adapters)
            Bots.AddRange(adapter.GetBots());

        SubscribeEvents();
        LoadPlugins();
    }

    #region 初始化步骤

    private void SubscribeEvents()
    {
        void WithCatch(Type pluginType, Func<Plugin, Task> subscriber, string? eventName = null) => Task.Run(() =>
        {
            try
            {
                var plugin = (Plugin)Services.GetRequiredService(pluginType);
                subscriber.Invoke(plugin).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                var logger = Services.GetRequiredService<ILoggerFactory>().CreateLogger(pluginType);
                logger.LogError(e, "Error occurred while handling {EventName}.", eventName ?? "event");
            }
        });

        foreach (var bot in Bots)
        {
            bot.OnMessageReceived += (_, e) => Task.Run(() =>
            {
                var middlewareCtx = new MiddlewareContext(this, bot, e.Message, null);
                ExecuteMiddlewares(middlewareCtx, 0); // Wait for all middlewares' execution
                middlewareCtx.ServiceScope.Dispose();
                if (middlewareCtx.Response is not null)
                    bot.SendMessage(e.Message, middlewareCtx.Response);
            });

            var ctx = new Context(bot);

            foreach (var pluginType in _pluginTypes)
            {
                bot.OnGuildInvited += (_, e) => WithCatch(pluginType,
                    plugin => plugin.OnGuildInvited(ctx, e),
                    nameof(bot.OnGuildInvited));

                bot.OnGuildJoinRequested += (_, e) => WithCatch(pluginType,
                    plugin => plugin.OnGuildJoinRequested(ctx, e),
                    nameof(bot.OnFriendRequested));

                bot.OnFriendRequested += (_, e) => WithCatch(pluginType,
                    plugin => plugin.OnFriendRequested(ctx, e),
                    nameof(bot.OnFriendRequested));
            }
        }

        // Subscribe bots' logging event
        foreach (var adapter in _adapters)
        {
            var adapterType = adapter.GetType();
            foreach (var bot in adapter.GetBots())
                bot.OnLogging += (_, e) =>
                    Services.GetRequiredService<ILoggerFactory>()
                        .CreateLogger(adapterType.FullName ?? adapterType.Name)
                        .Log((LogLevel)e.LogLevel, e.LogMessage);
        }

        Logger.LogDebug("All bot events subscribed.");
    }

    private void LoadPlugins()
    {
        var logger = Services.GetRequiredService<ILogger<PluginLoadContext>>();
        foreach (var pluginType in _pluginTypes)
        {
            var loadCtx = new PluginLoadContext(RootCommandNode, pluginType, logger);
            var plugin = (Plugin)Services.GetRequiredService(pluginType);

            loadCtx.LoadFromAttributes();
            // Fluent API can override attributes
            plugin.OnLoading(loadCtx).GetAwaiter().GetResult();

            loadCtx.LoadCommandAliases();
            loadCtx.LoadCommandShortcuts(StringShortcuts, RegexShortcuts);
        }
    }

    #endregion

    /// <summary>
    /// 按顺序执行中间件，遵循洋葱模型
    /// </summary>
    /// <param name="ctx">中间件上下文</param>
    /// <param name="index">中间件索引</param>
    private void ExecuteMiddlewares(MiddlewareContext ctx, int index)
    {
        try
        {
            if (_middlewares.Count < index + 1) return;
            _middlewares[index].Invoke(ctx, () => ExecuteMiddlewares(ctx, index + 1)).GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error occurred while processing middleware {MiddlewareName}.",
                _middlewares[index].Method.Name);
        }
    }

    /// <summary>
    /// 在最外层插入异步中间件
    /// </summary>
    /// <param name="middleware">中间件方法</param>
    public FlandreApp UseMiddleware(Func<MiddlewareContext, Action, Task> middleware)
    {
        _middlewares.Add(middleware);
        return this;
    }

    /// <summary>
    /// 在最外层插入同步中间件
    /// </summary>
    /// <param name="middleware">中间件方法</param>
    public FlandreApp UseMiddleware(Action<MiddlewareContext, Action> middleware)
    {
        return UseMiddleware((ctx, next) =>
        {
            middleware.Invoke(ctx, next);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// 运行应用实例
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        OnStarting?.Invoke(this, new AppStartingEvent());
        Logger.LogDebug("Starting app...");

        UseAssigneeCheckerMiddleware();
        UsePluginMessageEventMiddleware();
        UseCommandSessionMiddleware();
        UseCommandParserMiddleware();
        UseCommandInvokerMiddleware();

        await Task.WhenAll(_adapters.Select(adapter => adapter.Start()).ToArray());
        await _hostApp.StartAsync(cancellationToken);

        Logger.LogInformation("App started.");
        Logger.LogDebug("Total {AdapterCount} adapters, {BotCount} bots", _adapters.Count, Bots.Count);
        Logger.LogDebug(
            "Total {PluginCount} plugins, {CommandCount} commands, {StringShortcutCount} string shortcuts, {RegexShortcutCount} regex shortcuts, {MiddlewareCount} middlewares",
            _pluginTypes.Count, RootCommandNode.CountCommands(),
            StringShortcuts.Count, RegexShortcuts.Count, _middlewares.Count);
        OnReady?.Invoke(this, new AppReadyEvent());
    }

    /// <summary>
    /// 停止应用实例
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await Task.WhenAll(_adapters.Select(adapter => adapter.Stop()).ToArray());
        await _hostApp.StopAsync(cancellationToken);
        Logger.LogInformation("App stopped.");
        OnStopped?.Invoke(this, new AppStoppedEvent());
    }

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
        _hostApp.Dispose();
    }
}