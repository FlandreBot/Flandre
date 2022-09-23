using Flandre.Core.Common;
using Flandre.Core.Events.App;
using Flandre.Core.Utils;

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

    private readonly List<IBot> _bots = new();

    private readonly List<Plugin> _plugins = new();

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
                _bots.AddRange(adapter.GetBots());
                break;

            case Plugin plugin:
                _plugins.Add(plugin);
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
        foreach (var bot in _bots)
        foreach (var plugin in _plugins)
            bot.OnMessageReceived += (b, e) =>
            {
                var ctx = new MessageContext(this, b, e.Message);
                try
                {
                    plugin.OnMessageReceived(ctx);
                    var content = plugin.OnCommandParsing(ctx);
                    if (content is not null)
                        b.SendMessage(e.Message.SourceType,
                            e.Message.GuildId, e.Message.ChannelId, e.Message.Sender.Id, content);
                }
                catch (Exception exception)
                {
                    plugin.Logger.Error(exception);
                }
            };

        OnAppReady?.Invoke(this, new AppReadyEvent());

        // Ctrl+C
        Console.CancelKeyPress += (_, _) =>
        {
            Task.WaitAll(_adapters.ConvertAll(adapter => adapter.Stop()).ToArray());
            OnAppStopped?.Invoke(this, new AppStoppedEvent());
            Environment.Exit(0);
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            Logger.Error((Exception)args.ExceptionObject);

        Task.Delay(-1);
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