using System.Runtime.CompilerServices;
using Flandre.Core.Common;
using Flandre.Core.Events.App;
using Flandre.Core.Utils;

[assembly: InternalsVisibleTo("Flandre.Core.Tests")]
[assembly: InternalsVisibleTo("Flandre.TestKit")]

// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Flandre.Core;

/// <summary>
/// 应用基本框架
/// </summary>
public class FlandreApp
{
    private readonly List<IAdapter<IBot>> _adapters = new();

    internal readonly List<IBot> Bots = new();

    internal readonly List<Plugin> Plugins = new();

    private readonly CancellationTokenSource _appStopTokenSource = new();

    private readonly Dictionary<string, object> _commandMap = new();

    internal static Logger Logger { get; } = new("App");

    /// <summary>
    /// App 配置
    /// </summary>
    public AppConfig Config { get; }

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
    public FlandreApp(AppConfig? config = null)
    {
        Config = config ?? new AppConfig();
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
            case IAdapter<IBot> adapter:
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
        Logger.Info("Starting App...");

        OnAppStarting?.Invoke(this, new AppStartingEvent());

        // 启动所有适配器
        Task.WaitAll(_adapters.ConvertAll(adapter => adapter.Start()).ToArray());

        // 为所有模块注册消息事件
        SubscribeEvents();

        OnAppReady?.Invoke(this, new AppReadyEvent());

        // Ctrl+C
        Console.CancelKeyPress += (_, _) => Stop();

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            Logger.Error((Exception)args.ExceptionObject);

        Task.Delay(-1, _appStopTokenSource.Token);
    }

    /// <summary>
    /// 停止应用实例
    /// </summary>
    public void Stop()
    {
        Task.WaitAll(_adapters.ConvertAll(adapter => adapter.Stop()).ToArray());
        OnAppStopped?.Invoke(this, new AppStoppedEvent());
        _appStopTokenSource.Cancel();
    }

    private void SubscribeEvents()
    {
        foreach (var bot in Bots)
        {
            foreach (var plugin in Plugins)
            {
                var ctx = new Context(this, bot);

                bot.OnMessageReceived += (_, e) =>
                    plugin.OnMessageReceived(new MessageContext(this, bot, e.Message));
                bot.OnGuildInvited += (_, e) => plugin.OnGuildInvited(ctx, e);
                bot.OnGuildRequested += (_, e) => plugin.OnGuildRequested(ctx, e);
                bot.OnFriendRequested += (_, e) => plugin.OnFriendRequested(ctx, e);
            }

            bot.OnMessageReceived += (_, e) =>
                OnCommandParsing(new MessageContext(this, bot, e.Message));
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
                ctx.Bot.SendMessage(ctx.Message.SourceType, ctx.Message.GuildId, ctx.Message.ChannelId,
                    ctx.Message.Sender.Id, content);
            }

            if (commandStr == Config.CommandPrefix) return;
            var parser = new StringParser(commandStr.TrimStart(Config.CommandPrefix));
            var root = parser.Read(' ');

            var obj = _commandMap.GetValueOrDefault(root);

            switch (obj)
            {
                case null:
                    if (Config.CommandPrefix == "") return;
                    ctx.Bot.SendMessage(ctx.Message.SourceType, ctx.Message.GuildId, ctx.Message.ChannelId,
                        ctx.Message.Sender.Id, $"未找到指令：{root}。");
                    return;

                case Command command:
                    DealCommand(command, parser);
                    return;

                case Plugin plugin:
                {
                    if (!parser.IsEnd())
                    {
                        root = $"{root}.{parser.Read(' ')}";
                        obj = _commandMap.GetValueOrDefault(root);
                        switch (obj)
                        {
                            case null:
                                ctx.Bot.SendMessage(ctx.Message.SourceType, ctx.Message.GuildId, ctx.Message.ChannelId,
                                    ctx.Message.Sender.Id, $"未找到指令：{root}。");
                                return;
                            case Command cmd:
                                DealCommand(cmd, parser);
                                return;
                        }
                    }

                    ctx.Bot.SendMessage(ctx.Message.SourceType, ctx.Message.GuildId, ctx.Message.ChannelId,
                        ctx.Message.Sender.Id, plugin.GetHelp());
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 应用配置
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// 全局指令前缀
        /// </summary>
        public string CommandPrefix { get; set; } = "";
    }
}