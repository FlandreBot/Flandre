using Flandre.Framework.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flandre.Framework;

/// <summary>
/// 插件集合
/// </summary>
public interface IPluginCollection
{
    /// <summary>
    /// 插件使用的服务
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// 配置
    /// </summary>
    IConfiguration Configuration { get; }

    /// <summary>
    /// 添加插件
    /// </summary>
    /// <param name="pluginType">插件类型</param>
    IPluginCollection Add(Type pluginType);
}

internal sealed class PluginCollection : IPluginCollection
{
    public PluginCollection(IServiceCollection services, IConfiguration configuration)
    {
        Services = services;
        Configuration = configuration;
    }

    public List<Type> PluginTypes { get; } = new();

    public IServiceCollection Services { get; }

    public IConfiguration Configuration { get; }

    public IPluginCollection Add(Type pluginType)
    {
        PluginTypes.Add(pluginType);
        Services.AddScoped(pluginType);
        return this;
    }
}

/// <summary>
/// 插件集合扩展方法
/// </summary>
public static class PluginCollectionExtensions
{
    /// <summary>
    /// 添加插件
    /// </summary>
    /// <param name="plugins"></param>
    /// <typeparam name="TPlugin"></typeparam>
    /// <returns></returns>
    public static IPluginCollection Add<TPlugin>(this IPluginCollection plugins) where TPlugin : Plugin
    {
        return plugins.Add(typeof(TPlugin));
    }

    /// <summary>
    /// 添加一个带配置的插件
    /// </summary>
    /// <param name="plugins"></param>
    /// <param name="configuration"></param>
    /// <typeparam name="TPlugin"></typeparam>
    /// <typeparam name="TPluginOptions"></typeparam>
    /// <returns></returns>
    public static IPluginCollection Add<TPlugin, TPluginOptions>(this IPluginCollection plugins,
        IConfiguration configuration)
        where TPlugin : Plugin where TPluginOptions : class
    {
        plugins.Add<TPlugin>();
        plugins.Services.Configure<TPluginOptions>(configuration);
        return plugins;
    }

    /// <summary>
    /// 添加一个带配置的插件
    /// </summary>
    /// <param name="plugins"></param>
    /// <param name="action"></param>
    /// <typeparam name="TPlugin"></typeparam>
    /// <typeparam name="TPluginOptions"></typeparam>
    /// <returns></returns>
    public static IPluginCollection Add<TPlugin, TPluginOptions>(this IPluginCollection plugins,
        Action<TPluginOptions> action)
        where TPlugin : Plugin where TPluginOptions : class
    {
        plugins.Add<TPlugin>();
        plugins.Services.Configure(action);
        return plugins;
    }
}
