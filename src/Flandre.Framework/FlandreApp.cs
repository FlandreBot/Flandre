using System.Collections.Concurrent;
using System.Reflection;
using Flandre.Core.Common;
using Flandre.Framework.Attributes;
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
    private readonly List<Bot> _bots = new();
    private readonly List<Type> _pluginTypes;
    private readonly List<Func<MiddlewareContext, Action, Task>> _middlewares = new();
    private bool _isInternalMiddlewaresAdded;

    private int _commandCount;
    private int _aliasCount;
    private int _shortcutCount;

    public IServiceProvider Services => _hostApp.Services;
    public ILogger<FlandreApp> Logger { get; }

    internal static ILogger<FlandreApp>? InternalLogger { get; private set; }
    internal readonly Dictionary<string, Command> CommandMap = new();
    internal readonly Dictionary<string, Command> ShortcutMap = new();
    internal ConcurrentDictionary<string, string> GuildAssignees { get; } = new();

    public static FlandreAppBuilder CreateBuilder(string[]? args = null) => new(args);
    public static FlandreAppBuilder CreateBuilder(HostApplicationBuilderSettings? settings) => new(settings);

    internal FlandreApp(IHost hostApp, List<Type> pluginTypes, List<IAdapter> adapters)
    {
        _hostApp = hostApp;
        _appOptions = _hostApp.Services.GetRequiredService<IOptionsMonitor<FlandreAppOptions>>();
        _pluginTypes = pluginTypes;
        _adapters = adapters;

        foreach (var adapter in _adapters)
            _bots.AddRange(adapter.GetBots());

        Logger = Services.GetRequiredService<ILogger<FlandreApp>>();
        InternalLogger ??= Logger;

        SubscribeEvents();
        MapCommands();
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

        foreach (var bot in _bots)
        {
            bot.OnMessageReceived += (_, e) => Task.Run(() =>
            {
                var middlewareCtx = new MiddlewareContext(this, bot, e.Message, null);
                ExecuteMiddlewares(middlewareCtx, 0); // Wait for all middlewares' execution
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

    private void MapCommands()
    {
        foreach (var pluginType in _pluginTypes)
        foreach (var method in pluginType.GetMethods())
        {
            var cmdAttr = method.GetCustomAttribute<CommandAttribute>();
            if (cmdAttr is null) continue;

            var options = method.GetCustomAttributes<OptionAttribute>().ToList();
            var shortcuts = method.GetCustomAttributes<ShortcutAttribute>().ToList();
            var aliases = method.GetCustomAttributes<AliasAttribute>().ToList();

            var command = new Command(pluginType, cmdAttr, method, options, shortcuts, aliases);

            CommandMap[cmdAttr.Command] = command;
            _commandCount++;

            foreach (var shortcut in shortcuts)
            {
                ShortcutMap[shortcut.Shortcut] = command;
                _shortcutCount++;
            }

            foreach (var alias in aliases)
            {
                CommandMap[alias.Alias] = command;
                _aliasCount++;
            }
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
    /// 使用内置的核心中间件，包括群组代理检查、插件消息事件、以及核心的指令解析触发。
    /// </summary>
    /// <remarks>
    /// 启动应用时若未注册会自动注册，若多次调用只会注册一次。<br/>
    /// 可以手动调用该方法，并在后面插入新的中间件。
    /// </remarks>
    public FlandreApp UseInternalMiddlewares()
    {
        if (!_isInternalMiddlewaresAdded)
        {
            UseMiddleware(CheckGuildAssigneeMiddleware);
            UseMiddleware(PluginMessageEventMiddleware);
            UseMiddleware(ParseCommandMiddleware);
            _isInternalMiddlewaresAdded = true;
        }

        return this;
    }

    /// <summary>
    /// 运行应用实例，阻塞当前线程。
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        OnStarting?.Invoke(this, new AppStartingEvent());
        Logger.LogDebug("Starting app...");

        if (!_isInternalMiddlewaresAdded) UseInternalMiddlewares();

        await Task.WhenAll(_adapters.Select(adapter => adapter.Start()).ToArray());
        await _hostApp.StartAsync(cancellationToken);
        Console.CancelKeyPress += StopOnCancelKeyPress;

        Logger.LogInformation("App started.");
        Logger.LogDebug(
            "Total {BotCount} bots, {PluginCount} plugins, {CommandCount} commands, {ShortcutCount} shortcuts, {AliasCount} aliases, {MiddlewareCount} middlewares",
            _bots.Count, _pluginTypes.Count, _commandCount, _shortcutCount, _aliasCount, _middlewares.Count);
        OnReady?.Invoke(this, new AppReadyEvent());
    }

    /// <summary>
    /// 停止应用实例
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await Task.WhenAll(_adapters.Select(adapter => adapter.Stop()).ToArray());
        await _hostApp.StopAsync(cancellationToken);
        Console.CancelKeyPress -= StopOnCancelKeyPress;
        Logger.LogInformation("App stopped.");
        OnStopped?.Invoke(this, new AppStoppedEvent());
    }

    private void StopOnCancelKeyPress(object? sender, EventArgs e) =>
        StopAsync().GetAwaiter().GetResult();

    public void Dispose() => _hostApp.Dispose();
}