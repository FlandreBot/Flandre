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

    private readonly Dictionary<string, Command> _commandMap = new();

    internal static Logger Logger { get; } = new("App");

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

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            Logger.Error((Exception)args.ExceptionObject);
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
                    _commandMap[command.CommandInfo.Command] = command;
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

        OnAppStarting?.Invoke(this, new AppStartingEvent());

        Logger.Info("Starting App...");

        // 启动所有模块
        Task.WaitAll(_adapters.Select(adapter => adapter.Start()).ToArray());
        Task.WaitAll(Plugins.Select(plugin => Task.Run(async () =>
        {
            var e = new PluginStartingEvent(plugin);
            OnPluginStarting?.Invoke(this, e);
            if (!e.Cancelled) await plugin.Start();
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

            bot.OnMessageReceived += (_, e) => CatchAndLog(() =>
                OnCommandParsing(new MessageContext(this, bot, e.Message)));

            foreach (var plugin in Plugins)
            {
                bot.OnMessageReceived += (_, e) => CatchAndLog(() =>
                    plugin.OnMessageReceived(new MessageContext(this, bot, e.Message)));
                bot.OnGuildInvited += (_, e) => CatchAndLog(() =>
                    plugin.OnGuildInvited(ctx, e));
                bot.OnGuildJoinRequested += (_, e) => CatchAndLog(() =>
                    plugin.OnGuildRequested(ctx, e));
                bot.OnFriendRequested += (_, e) => CatchAndLog(() =>
                    plugin.OnFriendRequested(ctx, e));

                Logger.DefaultLoggingHandlers.Add(e =>
                    plugin.OnLoggerLogging(ctx, e));
            }
        }
    }

    private void OnCommandParsing(MessageContext ctx)
    {
        var commandStr = ctx.Message.GetText().Trim();

        if (commandStr.StartsWith(Config.CommandPrefix) && commandStr != Config.CommandPrefix)
        {
            var parser = new StringParser(commandStr.TrimStart(Config.CommandPrefix));

            var root = parser.SkipSpaces().Read(' ');

            while (true)
            {
                if (_commandMap.TryGetValue(root, out var command) &&
                    (parser.IsEnd() || !_commandMap.Keys.Any(cmd =>
                        cmd.StartsWith($"{root}.{parser.Peek(' ')}"))))
                {
                    var content = command.ParseCommand(ctx, parser);
                    if (content is null) return;
                    ctx.Bot.SendMessage(ctx.Message, content).Wait();
                    return;
                }

                if (parser.SkipSpaces().IsEnd()) break;
                root = $"{root}.{parser.Read(' ')}";
            }

            if (!Config.IgnoreUndefinedCommand.Equals("no", StringComparison.OrdinalIgnoreCase)) return;
            ctx.Bot.SendMessage(ctx.Message, $"未找到指令：{root}。").Wait();
        }
    }
}

/// <summary>
/// 应用配置
/// </summary>
public class FlandreAppConfig
{
    /// <summary>
    /// 全局指令前缀
    /// </summary>
    public string CommandPrefix { get; set; } = "";

    /// <summary>
    /// 忽略未定义指令的调用。可用值为：
    /// <br/> no - （默认）不忽略，调用未定义指令时发出警告信息
    /// <br/> root - 仅忽略根指令（顶级指令）
    /// <br/> all - 忽略所有
    /// </summary>
    public string IgnoreUndefinedCommand { get; set; } = "no";
}