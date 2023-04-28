using Flandre.Core.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Flandre.Framework;

/// <summary>
/// 适配器集合
/// </summary>
public interface IAdapterCollection
{
    /// <summary>
    /// 适配器使用的服务
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// 添加一个适配器
    /// </summary>
    /// <param name="adapter">适配器实例</param>
    IAdapterCollection Add(IAdapter adapter);
}

internal sealed class AdapterCollection : IAdapterCollection
{
    public IServiceCollection Services { get; }

    internal List<IAdapter> Adapters { get; } = new();

    internal AdapterCollection(IServiceCollection services)
    {
        Services = services;
    }

    public IAdapterCollection Add(IAdapter adapter)
    {
        Adapters.Add(adapter);
        return this;
    }
}
