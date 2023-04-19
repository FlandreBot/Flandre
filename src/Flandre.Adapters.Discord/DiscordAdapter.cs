using Flandre.Core.Common;

namespace Flandre.Adapters.Discord;

/// <summary>
/// Discord 适配器
/// </summary>
public sealed class DiscordAdapter : IAdapter
{
    private readonly List<DiscordBot> _bots = new();

    /// <summary>
    /// 构造 Discord 适配器
    /// </summary>
    /// <param name="config"></param>
    public DiscordAdapter(DiscordAdapterConfig config)
    {
        foreach (var bot in config.Bots)
        {
            _bots.Add(new DiscordBot(bot, config.Proxy));
        }
    }

    /// <inheritdoc cref="IAdapter.StartAsync"/>
    public Task StartAsync()
    {
        return Task.WhenAll(_bots.Select(bot => bot.StartAsync()));
    }

    /// <inheritdoc cref="IAdapter.StopAsync"/>
    public async Task StopAsync()
    {
    }

    /// <inheritdoc cref="IAdapter.GetBots"/>
    public IEnumerable<Bot> GetBots() => _bots;
}

/// <summary>
/// Discord 适配器配置
/// </summary>
public class DiscordAdapterConfig
{
    /// <summary>
    /// bot 配置列表
    /// </summary>
    public List<DiscordBotConfig> Bots { get; set; } = new();

    /// <summary>
    /// 代理地址，为空则不使用代理
    /// </summary>
    public string? Proxy { get; set; } = null;
}
