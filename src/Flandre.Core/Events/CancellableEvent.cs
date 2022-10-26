namespace Flandre.Core.Events;

/// <summary>
/// 可取消的事件
/// </summary>
public class CancellableEvent : BaseEvent
{
    /// <summary>
    /// 事件是否被取消。设置为 <c>true</c> 来取消事件。
    /// </summary>
    public bool IsCancelled { get; set; }
}