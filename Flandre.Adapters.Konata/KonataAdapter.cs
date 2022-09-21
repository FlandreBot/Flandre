using Flandre.Core;
using Flandre.Core.Common;
using Flandre.Core.Utils;

namespace Flandre.Adapters.Konata;

public class KonataAdapter : IAdapter<KonataBot>
{
    private readonly Logger _logger = new("KonataAdapter");
    private readonly List<KonataBot> _bots = new();
    private readonly KonataAdapterConfig _config;

    public KonataAdapter(KonataAdapterConfig config)
    {
        _config = config;

        _config.Bots.ForEach(bot =>
            _bots.Add(new KonataBot(bot, _logger)));
    }

    public async Task Start()
    {
        _logger.Info("Starting Konata Adapter...");
        await Task.WhenAll(_bots.ConvertAll(bot => bot.Start()));
        _logger.Info("Konata Adapter started.");
    }

    public async Task Stop()
    {
        await Task.WhenAll(_bots.ConvertAll(bot => bot.Stop()));
        _logger.Info("Konata Adapter stopped.");
    }

    public IEnumerable<KonataBot> GetBots()
    {
        return _bots;
    }
}

public class KonataAdapterConfig
{
    public KonataAdapterConfig()
    {
        Bots = new List<KonataBotConfig>();
    }

    public KonataAdapterConfig(List<KonataBotConfig> bots)
    {
        Bots = bots;
    }

    public List<KonataBotConfig> Bots { get; init; }
}

public static class FlandreAppExtensions
{
    public static FlandreApp UseKonataAdapter(this FlandreApp app, KonataAdapterConfig config)
    {
        return app.Use(new KonataAdapter(config));
    }
}