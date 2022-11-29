using Flandre.Core.Common;
using Flandre.Framework.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Flandre.Framework;

public sealed class FlandreAppBuilder
{
    /// <summary>
    /// 服务集合
    /// </summary>
    public IServiceCollection Services { get; }

    private readonly List<Type> _pluginTypes = new();

    private readonly List<IAdapter> _adapters = new();

    private readonly FlandreAppConfig _appConfig;

    public FlandreAppBuilder(FlandreAppConfig? appConfig = null)
    {
        _appConfig = appConfig ?? new FlandreAppConfig();
        Services = new ServiceCollection();

        Services.AddLogging(builder => { builder.AddConsole(); });
    }

    /// <summary>
    /// 使用一个不带配置的插件。
    /// </summary>
    /// <typeparam name="TPlugin">插件类型</typeparam>
    public FlandreAppBuilder UsePlugin<TPlugin>() where TPlugin : Plugin
        => UsePlugin<TPlugin, object?>(null);

    /// <summary>
    /// 使用一个带配置的插件
    /// </summary>
    /// <param name="config">插件配置对象</param>
    /// <typeparam name="TPlugin">带配置的插件类型</typeparam>
    public FlandreAppBuilder UsePlugin<TPlugin>(object? config) where TPlugin : Plugin
        => UsePlugin<TPlugin, object?>(config);

    /// <summary>
    /// 使用一个带配置的插件。<br/>
    /// 与 <see cref="UsePlugin{TPlugin}(object?)"/> 不同的是，本重载指定了配置类型作为参数类型，减小出错概率。
    /// </summary>
    /// <param name="config">插件配置对象</param>
    /// <typeparam name="TPlugin">带插件的配置类型</typeparam>
    /// <typeparam name="TPluginConfig">插件配置类型</typeparam>
    public FlandreAppBuilder UsePlugin<TPlugin, TPluginConfig>(TPluginConfig? config) where TPlugin : Plugin
    {
        var pluginType = typeof(TPlugin);
        _pluginTypes.Add(pluginType);
        Services.AddTransient(pluginType);

        if (config is not null)
            Services.AddSingleton(config.GetType(), config);

        return this;
    }

    public FlandreAppBuilder UseAdapter(IAdapter adapter)
    {
        _adapters.Add(adapter);
        return this;
    }

    /// <summary>
    /// 建造 <see cref="FlandreApp"/> 实例。
    /// </summary>
    public FlandreApp Build()
    {
        var sp = Services.BuildServiceProvider();
        return new FlandreApp(_appConfig, sp, _pluginTypes, _adapters);
    }
}