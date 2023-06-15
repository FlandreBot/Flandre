using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Flandre.Framework;

/// <summary>
/// 服务集合扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 配置 <see cref="FlandreAppOptions"/>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureFlandreApp(this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.Configure<FlandreAppOptions>(configuration);
    }

    /// <summary>
    /// 配置 <see cref="FlandreAppOptions"/>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureFlandreApp(this IServiceCollection services,
        Action<FlandreAppOptions> action)
    {
        return services.Configure(action);
    }
}
