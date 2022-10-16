namespace Flandre.Core.Events.Logger;

/// <summary>
/// 日志记录前触发的事件
/// </summary>
public class LoggerLoggingEvent : BaseEvent
{
    internal string? CustomMessage;

    internal bool Cancelled;

    /// <summary>
    /// 设置自定义日志信息，将覆盖默认行为
    /// </summary>
    public void SetCustomMessage(string message)
    {
        CustomMessage = message;
    }

    /// <summary>
    /// 取消此次日志
    /// </summary>
    public void Cancel() => Cancelled = true;
}