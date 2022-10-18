using System.Runtime.CompilerServices;
using Flandre.Core.Common;
using Flandre.Core.Events.App;
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

    private readonly Dictionary<string, object> _commandMap = new();

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

                if (plugin.PluginInfo.BaseCommand is not null)
                {
                    _commandMap[plugin.PluginInfo.BaseCommand] = plugin;
                    foreach (var command in plugin.Commands)
                        if (plugin.PluginInfo.BaseCommand == command.CommandInfo.Command)
                            _commandMap[command.CommandInfo.Command] = command;
                        else
                            _commandMap[$"{plugin.PluginInfo.BaseCommand}.{command.CommandInfo.Command}"] = command;
                }
                else
                {
                    foreach (var command in plugin.Commands)
                        _commandMap[command.CommandInfo.Command] = command;
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

        OnAppStarting?.Invoke(this, new AppStartingEvent());

        Logger.Info("Starting App...");

        // 启动所有适配器
        Task.WaitAll(_adapters.ConvertAll(adapter => adapter.Start()).ToArray());

        OnAppReady?.Invoke(this, new AppReadyEvent());

        _exitEvent.WaitOne();
    }

    /// <summary>
    /// 停止应用实例
    /// </summary>
    public void Stop()
    {
        Task.WaitAll(_adapters.ConvertAll(adapter => adapter.Stop()).ToArray());
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

        foreach (var bot in Bots)
        {
            var ctx = new Context(this, bot);

            bot.OnMessageReceived += (_, e) => CatchAndLog(() =>
                OnCommandParsing(new MessageContext(this, bot, e.Message)));

            foreach (var plugin in Plugins)
            {
                OnAppStarting += (_, e) => CatchAndLog(() =>
                    plugin.OnAppStarting(ctx, e));
                OnAppReady += (_, e) => CatchAndLog(() =>
                    plugin.OnAppReady(ctx, e));
                OnAppStopped += (_, e) => CatchAndLog(() =>
                    plugin.OnAppStopped(ctx, e));

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

        if (commandStr.StartsWith(Config.CommandPrefix))
        {
            void DealCommand(Command cmd, StringParser p)
            {
                var content = cmd.ParseCommand(ctx, p);
                if (content is null) return;
                ctx.Bot.SendMessage(ctx.Message, content).Wait();
            }

            if (commandStr == Config.CommandPrefix) return;
            var parser = new StringParser(commandStr.TrimStart(Config.CommandPrefix));
            var root = parser.Read(' ');

            var obj = _commandMap.GetValueOrDefault(root);

            switch (obj)
            {
                case null:
                    if (Config.CommandPrefix == "") return;
                    if (!Config.IgnoreUndefinedCommand.Equals("no", StringComparison.OrdinalIgnoreCase)) return;
                    ctx.Bot.SendMessage(ctx.Message, $"未找到指令：{root}。").Wait();
                    return;

                case Command command:
                    DealCommand(command, parser);
                    return;

                case Plugin plugin:
                {
                    if (!parser.IsEnd())
                    {
                        root = $"{root}.{parser.SkipSpaces().Read(' ')}";
                        obj = _commandMap.GetValueOrDefault(root);
                        switch (obj)
                        {
                            case null:
                                if (Config.IgnoreUndefinedCommand.Equals("all", StringComparison.OrdinalIgnoreCase))
                                    return;
                                ctx.Bot.SendMessage(ctx.Message, $"未找到指令：{root}。").Wait();
                                return;
                            case Command cmd:
                                DealCommand(cmd, parser);
                                return;
                        }
                    }

                    if (!Config.IgnoreUndefinedCommand.Equals("no", StringComparison.OrdinalIgnoreCase))
                        return;
                    ctx.Bot.SendMessage(ctx.Message, plugin.GetHelp()).Wait();
                    break;
                }
            }
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