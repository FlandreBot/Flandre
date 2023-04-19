namespace Flandre.Core.Common;

/// <summary>
/// 适配器接口
/// </summary>
public interface IAdapter
{
    /// <summary>
    /// 适配器机器人列表
    /// </summary>
    public IEnumerable<Bot> Bots { get; }
    
    /// <summary>
    /// 启动适配器
    /// </summary>
    public Task StartAsync();

    /// <summary>
    /// 停止适配器
    /// </summary>
    public Task StopAsync();
}
