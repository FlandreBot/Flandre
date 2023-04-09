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
    /// 添加插件
    /// </summary>
    /// <param name="pluginType">插件类型</param>
    IPluginCollection Add(Type pluginType);

    /// <summary>
    /// 添加插件
    /// </summary>
    /// <typeparam name="TPlugin">插件类型</typeparam>
    IPluginCollection Add<TPlugin>() where TPlugin : Plugin;

    /// <summary>
    /// 添加一个带配置的插件
    /// </summary>
    /// <param name="configuration"></param>
    /// <typeparam name="TPlugin"></typeparam>
    /// <typeparam name="TPluginOptions"></typeparam>
    /// <returns></returns>
    IPluginCollection Add<TPlugin, TPluginOptions>(IConfiguration configuration)
        where TPlugin : Plugin where TPluginOptions : class;

    /// <summary>
    /// 添加一个带配置的插件
    /// </summary>
    /// <param name="action"></param>
    /// <typeparam name="TPlugin"></typeparam>
    /// <typeparam name="TPluginOptions"></typeparam>
    /// <returns></returns>
    IPluginCollection Add<TPlugin, TPluginOptions>(Action<TPluginOptions> action)
        where TPlugin : Plugin where TPluginOptions : class;
}

internal sealed class PluginCollection : IPluginCollection
{
    public List<Type> PluginTypes { get; } = new();

    public IServiceCollection Services { get; } = new ServiceCollection();

    public IPluginCollection Add(Type pluginType)
    {
        PluginTypes.Add(pluginType);
        Services.AddScoped(pluginType);
        return this;
    }

    public IPluginCollection Add<TPlugin>() where TPlugin : Plugin
    {
        return Add(typeof(TPlugin));
    }

    public IPluginCollection Add<TPlugin, TPluginOptions>(IConfiguration configuration)
        where TPlugin : Plugin where TPluginOptions : class
    {
        Add<TPlugin>();
        Services.Configure<TPluginOptions>(configuration);
        return this;
    }

    public IPluginCollection Add<TPlugin, TPluginOptions>(Action<TPluginOptions> action)
        where TPlugin : Plugin where TPluginOptions : class
    {
        Add<TPlugin>();
        Services.Configure(action);
        return this;
    }
}
