namespace Flandre.Core.Common;

/// <summary>
/// 适配器接口
/// </summary>
public interface IAdapter
{
    /// <summary>
    /// 启动适配器
    /// </summary>
    public Task StartAsync();

    /// <summary>
    /// 停止适配器
    /// </summary>
    public Task StopAsync();

    /// <summary>
    /// 获取适配器 bot 列表
    /// </summary>
    public IEnumerable<Bot> GetBots();
}
