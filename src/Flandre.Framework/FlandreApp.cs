using System.Collections.Concurrent;
using Flandre.Core.Common;
using Flandre.Core.Messaging;
using Flandre.Framework.Common;
using Flandre.Framework.Events;
using Flandre.Framework.Routing;
using Flandre.Framework.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Flandre.Framework;

/// <summary>
/// Flandre 应用
/// </summary>
public sealed partial class FlandreApp : IHost, ICommandRouteBuilder
{
    private readonly IHost _hostApp;
    private readonly IOptionsMonitor<FlandreAppOptions> _appOptions;
    private readonly List<IAdapter> _adapters;
    private readonly List<Type> _pluginTypes;
    private readonly List<Func<MiddlewareContext, Func<Task>, Task>> _middleware = new();
    private readonly List<Bot> _bots = new();
    private bool _eventsSubscribedOnce;

    /// <summary>
    /// 所有机器人实例
    /// </summary>
    public IEnumerable<Bot> Bots => _bots.AsReadOnly();

    /// <summary>
    /// 服务
    /// </summary>
    public IServiceProvider Services => _hostApp.Services;

    /// <summary>
    /// 日志
    /// </summary>
    public ILogger<FlandreApp> Logger { get; }

    /// <summary>
    /// 应用属性，用于在中间件内传递消息
    /// </summary>
    public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();

    internal ConcurrentDictionary<string, string> GuildAssignees { get; } = new();
    internal ConcurrentDictionary<string, TaskCompletionSource<Message?>> CommandSessions { get; } = new();

    /// <summary>
    /// 创建构造器
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static FlandreAppBuilder CreateBuilder(string[]? args = null) => new(args);

    /// <summary>
    /// 创建构造器
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static FlandreAppBuilder CreateBuilder(HostApplicationBuilderSettings? settings) => new(settings);

    internal FlandreApp(IHost hostApp, List<Type> pluginTypes, List<IAdapter> adapters)
    {
        _hostApp = hostApp;
        _appOptions = _hostApp.Services.GetRequiredService<IOptionsMonitor<FlandreAppOptions>>();
        _pluginTypes = pluginTypes;
        _adapters = adapters;

        Logger = Services.GetRequiredService<ILogger<FlandreApp>>();

        foreach (var adapter in _adapters)
            _bots.AddRange(adapter.Bots);
    }

    private async Task LoadAllPluginsAsync()
    {
        foreach (var pluginType in _pluginTypes)
        {
            using var scope = Services.CreateScope();
            var loadCtx = new PluginLoadContext(pluginType, scope.ServiceProvider);
            var plugin = (Plugin)scope.ServiceProvider.GetRequiredService(pluginType);

            loadCtx.LoadFromAttributes();
            // Fluent API can override attributes
            await plugin.OnLoadingAsync();

            loadCtx.LoadCommandAliases();
            loadCtx.LoadCommandShortcuts();
        }
    }

    private async Task UnloadAllPluginsAsync()
    {
        foreach (var pluginType in _pluginTypes)
        {
            using var scope = Services.CreateScope();
            var plugin = (Plugin)scope.ServiceProvider.GetRequiredService(pluginType);
            await plugin.OnUnloadingAsync();
        }

        Services.GetRequiredService<CommandService>().Reset();
    }

    private void SubscribeEvents()
    {
        if (_eventsSubscribedOnce) return;
        _eventsSubscribedOnce = true;

        void WithCatch(Type pluginType, Func<Plugin, Task> subscriber, string? eventName = null) => Task.Run(async () =>
        {
            try
            {
                using var scope = Services.CreateScope();
                var plugin = (Plugin)scope.ServiceProvider.GetRequiredService(pluginType);
                await subscriber.Invoke(plugin);
            }
            catch (Exception e)
            {
                var logger = Services.GetRequiredService<ILoggerFactory>().CreateLogger(pluginType);
                logger.LogError(e, "Error occurred while handling {EventName}", eventName ?? "event");
            }
        });

        foreach (var bot in _bots)
        {
            bot.OnMessageReceived += (_, e) => Task.Run(async () =>
            {
                var middlewareCtx = new MiddlewareContext(this, bot, e.Message, null);
                await ExecuteMiddlewareAsync(middlewareCtx, 0); // Wait for all middleware's execution
                middlewareCtx.ServiceScope.Dispose();
                if (middlewareCtx.Response is not null)
                    await bot.SendMessageAsync(e.Message, middlewareCtx.Response);
            });

            var ctx = new BotContext(bot);

            foreach (var pluginType in _pluginTypes)
            {
                bot.OnGuildInvited += (_, e) => WithCatch(pluginType,
                    plugin => plugin.OnGuildInvitedAsync(ctx, e),
                    nameof(bot.OnGuildInvited));

                bot.OnGuildJoinRequested += (_, e) => WithCatch(pluginType,
                    plugin => plugin.OnGuildJoinRequestedAsync(ctx, e),
                    nameof(bot.OnFriendRequested));

                bot.OnFriendRequested += (_, e) => WithCatch(pluginType,
                    plugin => plugin.OnFriendRequestedAsync(ctx, e),
                    nameof(bot.OnFriendRequested));
            }
        }

        // Subscribe bots' logging event
        foreach (var adapter in _adapters)
        {
            var adapterType = adapter.GetType();
            foreach (var bot in adapter.Bots)
                bot.OnLogging += (_, e) =>
                    Services.GetRequiredService<ILoggerFactory>()
                        .CreateLogger(adapterType.FullName ?? adapterType.Name)
                        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                        .Log((LogLevel)e.LogLevel, e.LogMessage);
        }

        Logger.LogDebug("All bot events subscribed");
    }

    /// <summary>
    /// 按顺序执行中间件，遵循洋葱模型
    /// </summary>
    /// <param name="ctx">中间件上下文</param>
    /// <param name="index">中间件索引</param>
    private async Task ExecuteMiddlewareAsync(MiddlewareContext ctx, int index)
    {
        try
        {
            if (_middleware.Count < index + 1)
                return;
            await _middleware[index].Invoke(ctx, () => ExecuteMiddlewareAsync(ctx, index + 1));
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error occurred while processing middleware {MiddlewareName}",
                _middleware[index].Method.Name);
        }
    }

    /// <summary>
    /// 在最内层插入异步中间件
    /// </summary>
    /// <param name="middlewareAction">中间件方法</param>
    public FlandreApp Use(Func<MiddlewareContext, Func<Task>, Task> middlewareAction)
    {
        _middleware.Add(middlewareAction);
        return this;
    }

    private async Task StartAsyncCore(bool withDefaults, CancellationToken cancellationToken = default)
    {
        OnStarting?.Invoke(this, new AppStartingEvent());
        Logger.LogDebug("Starting app...");

        await LoadAllPluginsAsync();
        SubscribeEvents();

        UsePluginMessageHandler();

        if (withDefaults)
        {
            UseCommandSession();
            UseCommandParser();
            UseCommandInvoker();
        }

        await Task.WhenAll(_adapters.Select(adapter => adapter.StartAsync()).ToArray());
        await _hostApp.StartAsync(cancellationToken);

        var cmdService = Services.GetRequiredService<CommandService>();

        Logger.LogInformation("App started");
        Logger.LogDebug("Total {AdapterCount} adapters, {BotCount} bots", _adapters.Count, _bots.Count);
        Logger.LogDebug(
            "Total {PluginCount} plugins, {CommandCount} commands, {StringShortcutCount} string shortcuts, {RegexShortcutCount} regex shortcuts, {MiddlewareCount} middleware",
            _pluginTypes.Count, cmdService.RootCommandNode.CountCommands(),
            cmdService.StringShortcuts.Count, cmdService.RegexShortcuts.Count, _middleware.Count);
        OnReady?.Invoke(this, new AppReadyEvent());
    }

    /// <summary>
    /// 运行应用实例
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        return StartAsyncCore(false, cancellationToken);
    }

    /// <summary>
    /// 运行应用实例，并自动注册内置中间件
    /// </summary>
    public Task StartWithDefaultsAsync(CancellationToken cancellationToken = default)
    {
        return StartAsyncCore(true, cancellationToken);
    }

    /// <summary>
    /// 停止应用实例
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await UnloadAllPluginsAsync();
        await Task.WhenAll(_adapters.Select(adapter => adapter.StopAsync()).ToArray());
        await _hostApp.StopAsync(cancellationToken);
        Logger.LogInformation("App stopped");
        OnStopped?.Invoke(this, new AppStoppedEvent());
    }

    /// <summary>
    /// 停止应用实例并释放资源
    /// </summary>
    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
        _hostApp.Dispose();
    }
}
