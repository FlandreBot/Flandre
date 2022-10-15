namespace Flandre.Core.Common;

/// <summary>
/// 适配器接口
/// </summary>
public interface IAdapter : IModule
{
    /// <summary>
    /// 启动适配器
    /// </summary>
    public Task Start();

    /// <summary>
    /// 停止适配器
    /// </summary>
    public Task Stop();

    /// <summary>
    /// 获取适配器 bot 列表
    /// </summary>
    public IEnumerable<IBot> GetBots();
}