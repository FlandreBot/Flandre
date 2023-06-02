using Flandre.Core.Common;

namespace Flandre.Core.Events;

/// <summary>
/// Bot 日志记录事件
/// </summary>
public class BotLoggingEvent : FlandreEvent
{
    /// <summary>
    /// 日志等级
    /// </summary>
    public BotLogLevel LogLevel { get; }

    /// <summary>
    /// 日志消息
    /// </summary>
    public string LogMessage { get; }

    internal BotLoggingEvent(BotLogLevel level, string message)
    {
        LogLevel = level;
        LogMessage = message;
    }
}
