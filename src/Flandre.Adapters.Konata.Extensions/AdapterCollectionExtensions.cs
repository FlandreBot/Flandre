using Flandre.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flandre.Adapters.Konata.Extensions;

/// <summary>
/// Konata 适配器扩展
/// </summary>
public static class AdapterCollectionExtensions
{
    /// <summary>
    /// 添加 Konata 适配器，自动从配置根中的 <c>Adapters:Konata</c> 项读取配置。
    /// </summary>
    public static void AddKonata(this IAdapterCollection adapters)
    {
        var config = adapters.Services
            .BuildServiceProvider()
            .GetRequiredService<IConfigurationRoot>()
            .GetSection("Adapters:Konata")
            .Get<KonataAdapterConfig>();
        adapters.Add(new KonataAdapter(config ?? new KonataAdapterConfig()));
    }

    /// <summary>
    /// 添加 Konata 适配器。
    /// </summary>
    public static void AddKonata(this IAdapterCollection adapters, IConfiguration configuration)
    {
        var config = configuration.Get<KonataAdapterConfig>();
        adapters.Add(new KonataAdapter(config ?? new KonataAdapterConfig()));
    }

    /// <summary>
    /// 添加 Konata 适配器。
    /// </summary>
    public static void AddKonata(this IAdapterCollection adapters, Action<KonataAdapterConfig> action)
    {
        var config = new KonataAdapterConfig();
        action(config);
        adapters.Add(new KonataAdapter(config));
    }

    /// <summary>
    /// 添加 Konata 适配器。
    /// </summary>
    public static void AddKonata(this IAdapterCollection adapters, KonataAdapterConfig config)
    {
        adapters.Add(new KonataAdapter(config));
    }
}
