using Flandre.Core;
using Flandre.Core.Common;
using Flandre.Core.Utils;

namespace Flandre.Adapters.Konata;

/// <summary>
/// Konata 适配器
/// </summary>
public class KonataAdapter : IAdapter
{
    private readonly Logger _logger = new("KonataAdapter");
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
            _bots.Add(new KonataBot(bot, _logger)));
    }

    /// <summary>
    /// 启动适配器
    /// </summary>
    public async Task Start()
    {
        _logger.Info("Starting Konata Adapter...");
        await Task.WhenAll(_bots.ConvertAll(bot => bot.Start()));
        _logger.Info("Konata Adapter started.");
    }

    /// <summary>
    /// 停止适配器
    /// </summary>
    public async Task Stop()
    {
        await Task.WhenAll(_bots.ConvertAll(bot => bot.Stop()));
        _logger.Info("Konata Adapter stopped.");
    }

    /// <summary>
    /// 获取 bot 列表
    /// </summary>
    public IEnumerable<IBot> GetBots()
    {
        return _bots;
    }
}

/// <summary>
/// Konata 适配器配置
/// </summary>
public class KonataAdapterConfig
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

/// <summary>
/// FlandreApp 扩展方法
/// </summary>
public static class FlandreAppExtensions
{
    /// <summary>
    /// 使用 Konata 适配器
    /// </summary>
    /// <param name="app">FlandreApp 实例</param>
    /// <param name="config">Konata 适配器配置</param>
    public static FlandreApp UseKonataAdapter(this FlandreApp app, KonataAdapterConfig config)
    {
        return app.Use(new KonataAdapter(config));
    }
}