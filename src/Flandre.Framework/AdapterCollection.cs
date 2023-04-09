using Flandre.Core.Common;

namespace Flandre.Framework;

/// <summary>
/// 适配器集合
/// </summary>
public interface IAdapterCollection
{
    /// <summary>
    /// 添加一个适配器
    /// </summary>
    /// <param name="adapter">适配器实例</param>
    IAdapterCollection Add(IAdapter adapter);
}

internal sealed class AdapterCollection : IAdapterCollection
{
    internal List<IAdapter> Adapters { get; } = new();

    public IAdapterCollection Add(IAdapter adapter)
    {
        Adapters.Add(adapter);
        return this;
    }
}
