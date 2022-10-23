namespace Flandre.Core.Events;

/// <summary>
/// 可以取消的事件
/// </summary>
public interface ICancellableEvent
{
    /// <summary>
    /// 取消事件
    /// </summary>
    void Cancel();
}