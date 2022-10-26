namespace Flandre.Core.Events.Logger;

/// <summary>
/// 日志记录前触发的事件
/// </summary>
public class LoggerLoggingEvent : CancellableEvent
{
    /// <summary>
    /// 默认格式的日志信息，可覆盖。
    /// </summary>
    public string Message { get; set; }

    internal LoggerLoggingEvent(string message)
    {
        Message = message;
    }
}