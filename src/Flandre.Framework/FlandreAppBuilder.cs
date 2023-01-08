using Flandre.Core.Common;
using Flandre.Framework.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Flandre.Framework;

public sealed class FlandreAppBuilder
{
    private readonly HostApplicationBuilder _hostAppBuilder;
    private readonly List<Type> _pluginTypes = new();
    private readonly List<IAdapter> _adapters = new();

    public IServiceCollection Services => _hostAppBuilder.Services;
    public ConfigurationManager Configuration => _hostAppBuilder.Configuration;

    internal FlandreAppBuilder(string[]? args = null)
        : this(new HostApplicationBuilderSettings { Args = args })
    {
    }

    internal FlandreAppBuilder(HostApplicationBuilderSettings? settings)
    {
        _hostAppBuilder = new HostApplicationBuilder(settings);
    }

    /// <summary>
    /// 注册一个插件
    /// </summary>
    /// <typeparam name="TPlugin">插件类型</typeparam>
    public FlandreAppBuilder AddPlugin<TPlugin>() where TPlugin : Plugin
    {
        var pluginType = typeof(TPlugin);
        _pluginTypes.Add(pluginType);
        Services.AddTransient(pluginType);

        return this;
    }

    public FlandreAppBuilder AddPlugin<TPlugin, TPluginOptions>(IConfiguration configuration)
        where TPlugin : Plugin where TPluginOptions : class
    {
        Services.Configure<TPluginOptions>(configuration);
        return AddPlugin<TPlugin>();
    }

    public FlandreAppBuilder AddPlugin<TPlugin, TPluginOptions>(Action<TPluginOptions> action)
        where TPlugin : Plugin where TPluginOptions : class
    {
        Services.Configure(action);
        return AddPlugin<TPlugin>();
    }

    public FlandreAppBuilder AddAdapter(IAdapter adapter)
    {
        _adapters.Add(adapter);
        return this;
    }

    /// <summary>
    /// 建造 <see cref="FlandreApp"/> 实例。
    /// </summary>
    public FlandreApp Build()
    {
        return new FlandreApp(_hostAppBuilder.Build(), _pluginTypes, _adapters);
    }
}