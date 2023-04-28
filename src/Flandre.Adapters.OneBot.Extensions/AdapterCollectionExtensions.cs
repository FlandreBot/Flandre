using Flandre.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flandre.Adapters.OneBot.Extensions;

/// <summary>
/// OneBot 适配器扩展
/// </summary>
public static class AdapterCollectionExtensions
{
    /// <summary>
    /// 添加 OneBot 适配器，自动从配置根中的 <c>Adapters:OneBot</c> 项读取配置。
    /// </summary>
    public static void AddOneBot(this IAdapterCollection adapters)
    {
        var config = adapters.Services
            .BuildServiceProvider()
            .GetRequiredService<IConfigurationRoot>()
            .GetSection("Adapters:OneBot")
            .Get<OneBotAdapterConfig>();
        adapters.Add(new OneBotAdapter(config ?? new OneBotAdapterConfig()));
    }

    /// <summary>
    /// 添加 OneBot 适配器。
    /// </summary>
    public static void AddOneBot(this IAdapterCollection adapters, IConfiguration configuration)
    {
        var config = configuration.Get<OneBotAdapterConfig>();
        adapters.Add(new OneBotAdapter(config ?? new OneBotAdapterConfig()));
    }

    /// <summary>
    /// 添加 OneBot 适配器。
    /// </summary>
    public static void AddOneBot(this IAdapterCollection adapters, Action<OneBotAdapterConfig> action)
    {
        var config = new OneBotAdapterConfig();
        action(config);
        adapters.Add(new OneBotAdapter(config));
    }

    /// <summary>
    /// 添加 OneBot 适配器。
    /// </summary>
    public static void AddOneBot(this IAdapterCollection adapters, OneBotAdapterConfig config)
    {
        adapters.Add(new OneBotAdapter(config));
    }
}

