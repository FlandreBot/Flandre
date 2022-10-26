using System.Runtime.CompilerServices;
using Flandre.Core.Common;
using Flandre.Core.Events.App;
using Flandre.Core.Events.Plugin;
using Flandre.Core.Messaging;
using Flandre.Core.Utils;

[assembly: InternalsVisibleTo("Flandre.Core.Tests")]
[assembly: InternalsVisibleTo("Flandre.Adapters.Mock")]

// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Flandre.Core;

/// <summary>
/// 应用基本框架
/// </summary>
public class FlandreApp
{
    private readonly List<IAdapter> _adapters = new();

    internal readonly List<Bot> Bots = new();

    internal readonly List<Plugin> Plugins = new();

    private readonly ManualResetEvent _exitEvent = new(false);

    internal static Logger Logger { get; } = new("App");

    internal Dictionary<string, Command> CommandMap { get; } = new();

    internal Dictionary<string, Command> ShortcutMap { get; } = new();

    /// <summary>
    /// App 配置
    /// </summary>
    public FlandreAppConfig Config { get; }

    #region Events

    /// <summary>
    /// App 事件委托
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public delegate void AppEventHandler<in TEvent>(FlandreApp app, TEvent e);

    /// <summary>
    /// 应用准备启动事件
    /// </summary>
    public event AppEventHandler<AppStartingEvent>? OnAppStarting;

    /// <summary>
    /// 应用退出事件
    /// </summary>
    public event AppEventHandler<AppStoppedEvent>? OnAppStopped;

    /// <summary>
    /// 应用就绪事件
    /// </summary>
    public event AppEventHandler<AppReadyEvent>? OnAppReady;

    /// <summary>
    /// 插件启动事件
    /// </summary>
    public event AppEventHandler<PluginStartingEvent>? OnPluginStarting;

    /// <summary>
    /// 插件停止事件
    /// </summary>
    public event AppEventHandler<PluginStoppedEvent>? OnPluginStopped;

    /// <summary>
    /// 指令解析前事件
    /// </summary>
    public event AppEventHandler<AppCommandParsingEvent>? OnCommandParsing;

    /// <summary>
    /// 指令解析完成事件
    /// </summary>
    public event AppEventHandler<AppCommandParsedEvent>? OnCommandParsed;

    /// <summary>
    /// 指令调用前事件
    /// </summary>
    public event AppEventHandler<AppCommandInvokingEvent>? OnCommandInvoking;

    /// <summary>
    /// 指令调用完成事件
    /// </summary>
    public event AppEventHandler<AppCommandInvokedEvent>? OnCommandInvoked;

    #endregion

    /// <summary>
    /// 构造应用实例
    /// </summary>
    /// <param name="config">应用配置</param>
    public FlandreApp(FlandreAppConfig? config = null)
    {
        Config = config ?? new FlandreAppConfig();

        // Ctrl+C
        Console.CancelKeyPress += (_, _) => Stop();

        // AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        //     Logger.Error((Exception)args.ExceptionObject);
    }

    /// <summary>
    /// 注册模块
    /// </summary>
    /// <param name="module">需要注册的模块</param>
    /// <returns>应用实例本身</returns>
    public FlandreApp Use(IModule module)
    {
        switch (module)
        {
            case IAdapter adapter:
                _adapters.Add(adapter);
                Bots.AddRange(adapter.GetBots());
                break;

            case Plugin plugin:
                Plugins.Add(plugin);
                foreach (var command in plugin.Commands)
                {
                    CommandMap[command.CommandInfo.Command] = command;
                    foreach (var shortcut in command.Shortcuts)
                        ShortcutMap[shortcut.Shortcut] = command;
                    foreach (var alias in command.Aliases)
                        CommandMap[alias.Alias] = command;
                }

                break;
        }

        return this;
    }

    /// <summary>
    /// 启动应用实例
    /// </summary>
    public void Start()
    {
        SubscribeEvents(); // 注册事件

        var startingEvent = new AppStartingEvent();
        OnAppStarting?.Invoke(this, startingEvent);
        if (startingEvent.IsCancelled) return;

        Logger.Info("Starting App...");

        // 启动所有模块
        Task.WaitAll(_adapters.Select(adapter => adapter.Start()).ToArray());
        Task.WaitAll(Plugins.Select(plugin => Task.Run(async () =>
        {
            var e = new PluginStartingEvent(plugin);
            OnPluginStarting?.Invoke(this, e);
            if (!e.IsCancelled) await plugin.Start();
        })).ToArray());

        OnAppReady?.Invoke(this, new AppReadyEvent());

        _exitEvent.WaitOne();
    }

    /// <summary>
    /// 停止应用实例
    /// </summary>
    public void Stop()
    {
        Task.WaitAll(_adapters.Select(adapter => adapter.Stop()).ToArray());
        Task.WaitAll(Plugins.Select(plugin => Task.Run(async () =>
        {
            await plugin.Stop();
            OnPluginStopped?.Invoke(this, new PluginStoppedEvent(plugin));
        })).ToArray());
        OnAppStopped?.Invoke(this, new AppStoppedEvent());
        _exitEvent.Set();
    }

    private void SubscribeEvents()
    {
        void CatchAndLog(Action action) => Task.Run(() =>
        {
            try
            {
                action();
            }
            catch (AggregateException ae)
            {
                Logger.Error(ae.InnerException ?? ae);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        });

        foreach (var plugin in Plugins)
        {
            OnAppStarting += plugin.OnAppStarting;
            OnAppReady += plugin.OnAppReady;
            OnAppStopped += plugin.OnAppStopped;
        }

        foreach (var bot in Bots)
        {
            var ctx = new Context(this, bot);

            foreach (var plugin in Plugins)
            {
                Logger.DefaultLoggingHandlers.Add(e =>
                    plugin.OnLoggerLogging(ctx, e));

                bot.OnMessageReceived += (_, e) => CatchAndLog(() =>
                    plugin.OnMessageReceived(new MessageContext(this, bot, e.Message)));
                bot.OnGuildInvited += (_, e) => CatchAndLog(() =>
                    plugin.OnGuildInvited(ctx, e));
                bot.OnGuildJoinRequested += (_, e) => CatchAndLog(() =>
                    plugin.OnGuildJoinRequested(ctx, e));
                bot.OnFriendRequested += (_, e) => CatchAndLog(() =>
                    plugin.OnFriendRequested(ctx, e));
            }

            bot.OnMessageReceived += (_, e) => CatchAndLog(() =>
                ParseCommand(new MessageContext(this, bot, e.Message)));
        }
    }

    private void ParseCommand(MessageContext ctx)
    {
        void ParseAndInvoke(Command c, StringParser p)
        {
            var parsingEvent = new AppCommandParsingEvent(ctx, c, p);
            OnCommandParsing?.Invoke(this, parsingEvent);
            if (parsingEvent.IsCancelled) return;
            var args = parsingEvent.CustomArgs;
            if (args is null)
            {
                (args, var error) = c.ParseCommand(p);
                OnCommandParsed?.Invoke(this, new AppCommandParsedEvent(args, error));
                if (error is not null)
                {
                    ctx.Bot.SendMessage(ctx.Message, error).Wait();
                    return;
                }
            }

            var invokingEvent = new AppCommandInvokingEvent(c, ctx, args);
            OnCommandInvoking?.Invoke(this, invokingEvent);

            var content = c.InvokeCommand(ctx, args);

            var invokedEvent = new AppCommandInvokedEvent(c, ctx, args, content);
            OnCommandInvoked?.Invoke(this, invokedEvent);

            if (content is null) return;
            ctx.Bot.SendMessage(ctx.Message, content).Wait();
        }

        var commandStr = ctx.Message.GetText().Trim();
        if (commandStr == Config.CommandPrefix) return;

        var parser = new StringParser(commandStr);

        var root = parser.SkipSpaces().Read(' ');

        if (ShortcutMap.TryGetValue(root, out var command))
        {
            ParseAndInvoke(command, parser);
            return;
        }

        root = root.TrimStart(Config.CommandPrefix);
        parser.SkipSpaces();

        while (true)
        {
            if (CommandMap.TryGetValue(root, out command) &&
                (parser.IsEnd() || !CommandMap.Keys.Any(cmd =>
                    cmd.StartsWith($"{root}.{parser.Peek(' ')}"))))
            {
                ParseAndInvoke(command, parser);
                return;
            }

            if (parser.SkipSpaces().IsEnd()) break;
            root = $"{root}.{parser.Read(' ')}";
        }

        if (string.IsNullOrWhiteSpace(Config.CommandPrefix)) return;
        if (!Config.IgnoreUndefinedCommand.Equals("no", StringComparison.OrdinalIgnoreCase)) return;
        ctx.Bot.SendMessage(ctx.Message, $"未找到指令：{root}。").Wait();
    }
}