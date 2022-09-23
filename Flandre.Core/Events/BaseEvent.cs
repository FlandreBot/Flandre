namespace Flandre.Core.Events;

/// <summary>
/// 基础事件
/// </summary>
public class BaseEvent : EventArgs
{
    /// <summary>
    /// 事件时间
    /// </summary>
    public DateTime EventTime { get; }

    internal BaseEvent()
    {
        EventTime = DateTime.Now;
    }
}