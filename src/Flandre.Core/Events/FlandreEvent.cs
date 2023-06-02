namespace Flandre.Core.Events;

/// <summary>
/// 基础事件
/// </summary>
public abstract class FlandreEvent : EventArgs
{
    /// <summary>
    /// 事件时间
    /// </summary>
    public DateTime EventTime { get; init; }

    /// <summary>
    /// 事件载荷
    /// </summary>
    public object? EventPayload { get; init; }

    internal FlandreEvent()
    {
        EventTime = DateTime.Now;
    }
}
