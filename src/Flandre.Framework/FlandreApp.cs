using System.Collections.Concurrent;
using System.Reflection;
using Flandre.Core.Common;
using Flandre.Framework.Attributes;
using Flandre.Framework.Common;
using Flandre.Framework.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Flandre.Framework;

public sealed partial class FlandreApp
{
    public IServiceProvider Services { get; }
    public ILogger<FlandreApp> Logger { get; }

    internal static ILogger<FlandreApp>? InternalLogger { get; private set; }

    private readonly FlandreAppConfig _config;

    private readonly List<IAdapter> _adapters;
    private readonly List<Bot> _bots = new();
    private readonly List<Type> _pluginTypes;
    private readonly List<Func<MiddlewareContext, Action, Task>> _middlewares = new();

    internal readonly Dictionary<string, Command> CommandMap = new();
    internal readonly Dictionary<string, Command> ShortcutMap = new();

    internal ConcurrentDictionary<string, string> GuildAssignees { get; } = new();

    private readonly ManualResetEvent _exitEvent = new(false);

    internal FlandreApp(FlandreAppConfig config, IServiceProvider serviceProvider,
        List<Type> pluginTypes, List<IAdapter> adapters)
    {
        _config = config;
        Services = serviceProvider;
        _pluginTypes = pluginTypes;
        _adapters = adapters;

        foreach (var adapter in _adapters)
            _bots.AddRange(adapter.GetBots());

        Logger = Services.GetRequiredService<ILogger<FlandreApp>>();
        InternalLogger ??= Logger;

        Console.CancelKeyPress += (_, _) => Stop();
    }

    #region 初始化步骤

    private void SubscribeEvents()
    {
        void WithCatch(Type pluginType, Func<Plugin, Task> subscriber, string? eventName = null) => Task.Run(() =>
        {
            try
            {
                var plugin = (Plugin)Services.GetRequiredService(pluginType);
                subscriber.Invoke(plugin).Wait();
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

            Logger.LogDebug("All bot events subscribed.");
        }
    }

    private void MapCommands()
    {
        var pluginCount = 0;
        var commandCount = 0;
        var aliasCount = 0;
        var shortcutCount = 0;
        foreach (var pluginType in _pluginTypes)
        {
            pluginCount++;
            foreach (var method in pluginType.GetMethods())
            {
                var cmdAttr = method.GetCustomAttribute<CommandAttribute>();
                if (cmdAttr is null) continue;

                var options = method.GetCustomAttributes<OptionAttribute>().ToList();
                var shortcuts = method.GetCustomAttributes<ShortcutAttribute>().ToList();
                var aliases = method.GetCustomAttributes<AliasAttribute>().ToList();

                var command = new Command(pluginType, cmdAttr, method, options, shortcuts, aliases);

                CommandMap[cmdAttr.Command] = command;
                commandCount++;

                foreach (var shortcut in shortcuts)
                {
                    ShortcutMap[shortcut.Shortcut] = command;
                    shortcutCount++;
                }

                foreach (var alias in aliases)
                {
                    CommandMap[alias.Alias] = command;
                    aliasCount++;
                }
            }
        }

        Logger.LogDebug(
            "Total {PluginCount} plugins, {CommandCount} commands, {ShortcutCount} shortcuts, {AliasCount} aliases mapped.",
            pluginCount, commandCount, shortcutCount, aliasCount);
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
            _middlewares[index].Invoke(ctx, () => ExecuteMiddlewares(ctx, index + 1)).Wait();
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
        _middlewares.Insert(0, middleware);
        return this;
    }

    /// <summary>
    /// 在最外层插入同步中间件
    /// </summary>
    /// <param name="middleware">中间件方法</param>
    public FlandreApp UseMiddleware(Action<MiddlewareContext, Action> middleware)
    {
        _middlewares.Insert(0, (ctx, next) =>
        {
            middleware.Invoke(ctx, next);
            return Task.CompletedTask;
        });
        return this;
    }

    /// <summary>
    /// 运行应用实例，阻塞当前线程。
    /// </summary>
    public void Run()
    {
        OnStarting?.Invoke(this, new AppStartingEvent());
        Logger.LogDebug("Starting app...");
        Logger.LogDebug("Total bots: {BotCount}, total plugins: {PluginCount}",
            _bots.Count, _pluginTypes.Count);

        // Subscribe bots' logging event
        foreach (var adapter in _adapters)
        foreach (var bot in adapter.GetBots())
        {
            var bt = bot.GetType();
            bot.OnLogging += (_, e) =>
                Services.GetRequiredService<ILoggerFactory>()
                    .CreateLogger(bt.FullName ?? bt.Name)
                    .Log((LogLevel)e.LogLevel, e.LogMessage);
        }

        Task.WaitAll(_adapters.Select(adapter => adapter.Start()).ToArray());

        SubscribeEvents();
        MapCommands();

        UseMiddleware(ParseCommandMiddleware);
        UseMiddleware(PluginMessageEventMiddleware);
        UseMiddleware(CheckGuildAssigneeMiddleware);

        Logger.LogInformation("App started.");
        OnReady?.Invoke(this, new AppReadyEvent());

        _exitEvent.WaitOne();
    }

    /// <summary>
    /// 停止应用实例
    /// </summary>
    public void Stop()
    {
        Task.WaitAll(_adapters.Select(adapter => adapter.Stop()).ToArray());
        Logger.LogInformation("App stopped.");
        OnStopped?.Invoke(this, new AppStoppedEvent());
        _exitEvent.Set();
    }
}