using Flandre.Core.Common;
using Flandre.Core.Utils;

namespace Flandre.Adapters.OneBot;

public class OneBotAdapter : IAdapter
{
    private readonly List<IBot> _bots = new();

    private readonly OneBotAdapterConfig _config;

    private readonly Logger _logger = new("OneBotAdapter");

    public OneBotAdapter(OneBotAdapterConfig config)
    {
        _config = config;

        foreach (var bot in _config.Bots)
            switch (bot.Protocol.ToLower())
            {
                case "websocket":
                case "ws":
                    var obb = new OneBotWebSocketBot(bot, _logger);
                    _bots.Add(obb);
                    _bots.Add(obb.GuildBot);
                    break;

                default:
                    _logger.Warning($"OneBotAdapter 仅支持 websocket / ws 协议。正在跳过 Bot {bot.SelfId} 的初始化。");
                    break;
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

    public IEnumerable<IBot> GetBots()
    {
        return _bots;
    }
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