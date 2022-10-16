namespace Flandre.Core.Events.Logger;

/// <summary>
/// 日志记录前触发的事件
/// </summary>
public class LoggerLoggingEvent : BaseEvent
{
    /// <summary>
    /// 默认格式的日志信息
    /// </summary>
    public string Message { get; private set; }

    internal bool Cancelled;

    internal LoggerLoggingEvent(string message)
    {
        Message = message;
    }

    /// <summary>
    /// 设置自定义日志信息，将覆盖默认行为
    /// </summary>
    public void SetCustomMessage(string message)
    {
        Message = message;
    }

    /// <summary>
    /// 取消此次日志
    /// </summary>
    public void Cancel() => Cancelled = true;
}