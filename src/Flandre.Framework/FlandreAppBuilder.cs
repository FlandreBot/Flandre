using Flandre.Core.Common;
using Flandre.Framework.Common;
using Flandre.Framework.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Flandre.Framework;

/// <summary>
/// <see cref="FlandreApp"/> 构建器
/// </summary>
public sealed class FlandreAppBuilder
{
    private readonly HostApplicationBuilder _hostAppBuilder;
    private readonly List<Type> _pluginTypes = new();
    private readonly List<IAdapter> _adapters = new();

    /// <summary>
    /// 全局服务
    /// </summary>
    public IServiceCollection Services => _hostAppBuilder.Services;

    /// <summary>
    /// 配置
    /// </summary>
    public ConfigurationManager Configuration => _hostAppBuilder.Configuration;

    /// <summary>
    /// 环境
    /// </summary>
    public IHostEnvironment Environment => _hostAppBuilder.Environment;

    /// <summary>
    /// 日志
    /// </summary>
    public ILoggingBuilder Logging => _hostAppBuilder.Logging;

    /// <summary>
    /// 配置容器
    /// </summary>
    /// <typeparam name="TContainerBuilder"></typeparam>
    /// <param name="factory"></param>
    /// <param name="configure"></param>
    public void ConfigureContainer<TContainerBuilder>(
        IServiceProviderFactory<TContainerBuilder> factory, Action<TContainerBuilder>? configure = null)
        where TContainerBuilder : notnull
        => _hostAppBuilder.ConfigureContainer(factory, configure);

    internal FlandreAppBuilder(string[]? args = null)
        : this(new HostApplicationBuilderSettings { Args = args })
    {
    }

    internal FlandreAppBuilder(HostApplicationBuilderSettings? settings)
    {
        _hostAppBuilder = new HostApplicationBuilder(settings);
        AddInfrastructure();
    }

    private void AddInfrastructure()
    {
        Services.AddSingleton<CommandService>();
    }

    /// <summary>
    /// 添加插件
    /// </summary>
    /// <typeparam name="TPlugin">插件类型</typeparam>
    public FlandreAppBuilder AddPlugin<TPlugin>() where TPlugin : Plugin
    {
        var pluginType = typeof(TPlugin);
        _pluginTypes.Add(pluginType);
        Services.AddTransient(pluginType);

        return this;
    }

    /// <inheritdoc cref="AddPlugin{TPlugin}"/>
    /// <typeparam name="TPlugin"/>
    /// <typeparam name="TPluginOptions"></typeparam>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public FlandreAppBuilder AddPlugin<TPlugin, TPluginOptions>(IConfiguration configuration)
        where TPlugin : Plugin where TPluginOptions : class
    {
        Services.Configure<TPluginOptions>(configuration);
        return AddPlugin<TPlugin>();
    }

    /// <inheritdoc cref="AddPlugin{TPlugin}"/>
    /// <typeparam name="TPlugin"/>
    /// <typeparam name="TPluginOptions"></typeparam>
    /// <param name="action"></param>
    /// <returns></returns>
    public FlandreAppBuilder AddPlugin<TPlugin, TPluginOptions>(Action<TPluginOptions> action)
        where TPlugin : Plugin where TPluginOptions : class
    {
        Services.Configure(action);
        return AddPlugin<TPlugin>();
    }

    /// <summary>
    /// 添加机器人适配器
    /// </summary>
    /// <param name="adapter"></param>
    /// <returns></returns>
    public FlandreAppBuilder AddAdapter(IAdapter adapter)
    {
        _adapters.Add(adapter);
        return this;
    }

    /// <summary>
    /// 构建 <see cref="FlandreApp"/> 实例
    /// </summary>
    public FlandreApp Build()
    {
        var app = new FlandreApp(_hostAppBuilder.Build(), _pluginTypes, _adapters);
        app.Initialize();
        return app;
    }
}
