using Flandre.Core.Common;

namespace Flandre.Adapters.Konata;

/// <summary>
/// Konata 适配器
/// </summary>
public class KonataAdapter : IAdapter
{
    /// <inheritdocs/>
    public IEnumerable<Bot> Bots => _bots.AsReadOnly();

    private readonly List<KonataBot> _bots = new();
    private readonly KonataAdapterConfig _config;

    /// <summary>
    /// 构造适配器实例
    /// </summary>
    /// <param name="config">适配器配置</param>
    public KonataAdapter(KonataAdapterConfig config)
    {
        _config = config;

        _config.Bots.ForEach(bot =>
            _bots.Add(new KonataBot(bot)));
    }

    /// <summary>
    /// 启动适配器
    /// </summary>
    public Task StartAsync() => Task.WhenAll(_bots.ConvertAll(bot => bot.StartAsync()));

    /// <summary>
    /// 停止适配器
    /// </summary>
    public Task StopAsync() => Task.WhenAll(_bots.ConvertAll(bot => bot.StopAsync()));
}

/// <summary>
/// Konata 适配器配置
/// </summary>
public sealed class KonataAdapterConfig
{
    /// <summary>
    /// 构造 Konata 适配器配置
    /// </summary>
    public KonataAdapterConfig()
    {
        Bots = new List<KonataBotConfig>();
    }

    /// <summary>
    /// 构造 Konata 适配器配置，并使用已有的 bot 配置列表
    /// </summary>
    /// <param name="bots"></param>
    public KonataAdapterConfig(List<KonataBotConfig> bots)
    {
        Bots = bots;
    }

    /// <summary>
    /// bot 配置列表
    /// </summary>
    public List<KonataBotConfig> Bots { get; init; }
}
