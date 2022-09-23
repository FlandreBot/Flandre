namespace Flandre.Core.Common;

/// <summary>
/// 适配器接口
/// </summary>
/// <typeparam name="TBot"></typeparam>
public interface IAdapter<out TBot> : IModule where TBot : IBot
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
    public IEnumerable<TBot> GetBots();
}