using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flandre.Framework.Extensions;

/// <summary>
/// 服务集合扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureFlandreApp(this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.Configure<FlandreAppOptions>(configuration);
    }

    public static IServiceCollection ConfigureFlandreApp(this IServiceCollection services,
        Action<FlandreAppOptions> action)
    {
        return services.Configure(action);
    }
}
