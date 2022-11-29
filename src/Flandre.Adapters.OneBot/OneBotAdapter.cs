using Flandre.Core.Common;

namespace Flandre.Adapters.OneBot;

public class OneBotAdapter : IAdapter
{
    private readonly List<Bot> _bots = new();

    private readonly OneBotAdapterConfig _config;

    public OneBotAdapter(OneBotAdapterConfig config)
    {
        _config = config;

        foreach (var bot in _config.Bots)
            switch (bot.Protocol.ToLower())
            {
                case "websocket":
                case "ws":
                    var obb = new OneBotWebSocketBot(bot);
                    _bots.Add(obb);
                    _bots.Add(obb.GuildBot);
                    break;

                default:
                    throw new NotSupportedException(
                        $"OneBot adapter only supports \"websocket\" / \"ws\" protocol. Skipping initialization of bot {bot.SelfId}...");
            }
    }

    public async Task Start()
    {
        await Task.WhenAll(_bots.ConvertAll(bot => bot.Start()));
    }

    public async Task Stop()
    {
        await Task.WhenAll(_bots.ConvertAll(bot => bot.Stop()));
    }

    public IEnumerable<Bot> GetBots() => _bots;
}

public class OneBotAdapterConfig
{
    /// <summary>
    /// 构造 OneBot 适配器配置
    /// </summary>
    public OneBotAdapterConfig()
    {
        Bots = new List<OneBotBotConfig>();
    }

    /// <summary>
    /// 构造 OneBot 适配器配置，并使用已有的 bot 配置列表
    /// </summary>
    /// <param name="bots"></param>
    public OneBotAdapterConfig(List<OneBotBotConfig> bots)
    {
        Bots = bots;
    }

    /// <summary>
    /// bot 配置列表
    /// </summary>
    public List<OneBotBotConfig> Bots { get; init; }
}