using Flandre.Core.Common;
using Flandre.Core.Events.App;
using Flandre.Core.Utils;

// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Flandre.Core;

public class FlandreApp
{
    private readonly List<IAdapter<IBot>> _adapters = new();

    private readonly List<IBot> _bots = new();

    private readonly List<Plugin> _plugins = new();

    internal static Logger Logger { get; } = new("App");

    public AppConfig Config { get; }

    #region Events

    public delegate void AppEventHandler<in TEvent>(FlandreApp app, TEvent e);

    public event AppEventHandler<AppStartingEvent>? OnAppStarting;
    public event AppEventHandler<AppStoppedEvent>? OnAppStopped;
    public event AppEventHandler<AppReadyEvent>? OnAppReady;

    #endregion

    public FlandreApp(AppConfig? config = null)
    {
        Config = config ?? new AppConfig();
    }

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
                var ctx = new Context(this, b, e.Message);
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

public class AppConfig
{
    public string CommandPrefix { get; set; } = "";
}