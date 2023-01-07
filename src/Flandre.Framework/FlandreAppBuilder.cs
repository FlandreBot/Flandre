using Flandre.Core.Common;
using Flandre.Framework.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Flandre.Framework;

public sealed class FlandreAppBuilder
{
    private readonly HostApplicationBuilder _hostAppBuilder;
    private readonly List<Type> _pluginTypes = new();
    private readonly List<IAdapter> _adapters = new();

    public IServiceCollection Services => _hostAppBuilder.Services;

    internal FlandreAppBuilder(string[]? args = null)
    {
        _hostAppBuilder = Host.CreateApplicationBuilder(args);
    }

    /// <summary>
    /// 注册一个插件。
    /// </summary>
    /// <typeparam name="TPlugin">带插件的配置类型</typeparam>
    public FlandreAppBuilder UsePlugin<TPlugin>() where TPlugin : Plugin
    {
        var pluginType = typeof(TPlugin);
        _pluginTypes.Add(pluginType);
        Services.AddTransient(pluginType);

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
        return new FlandreApp(_hostAppBuilder.Build(), _pluginTypes, _adapters);
    }
}