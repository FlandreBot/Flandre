namespace Flandre.Core.Events;

/// <summary>
/// 基础事件
/// </summary>
public class BaseEvent : EventArgs
{
    /// <summary>
    /// 事件时间
    /// </summary>
    public DateTime EventTime { get; init; }

    /// <summary>
    /// 事件附带消息
    /// </summary>
    public object? EventMessage { get; init; }

    internal BaseEvent()
    {
        EventTime = DateTime.Now;
    }
}
