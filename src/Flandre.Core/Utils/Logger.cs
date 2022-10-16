using Flandre.Core.Events.Logger;

namespace Flandre.Core.Utils;

/// <summary>
/// 日志记录器
/// </summary>
public class Logger
{
    internal static List<Action<LoggerLoggingEvent>> DefaultLoggingHandlers = new();

    /// <summary>
    /// 日志类别
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 构造日志记录器
    /// </summary>
    /// <param name="name">日志类别</param>
    public Logger(string name)
    {
        Name = name;
    }

    /// <summary>
    /// 记录日志
    /// </summary>
    /// <param name="logLevel">日志等级</param>
    /// <param name="message">日志消息</param>
    protected void Log(LogLevels logLevel, string message)
    {
        var loggingEvent = new LoggerLoggingEvent();
        
        DefaultLoggingHandlers.ForEach(logging => logging(loggingEvent));
        OnLoggerLogging?.Invoke(this, loggingEvent);

        if (loggingEvent.Cancelled) return;
        
        var logMessage = loggingEvent.CustomMessage ??
                         $"{DateTime.Now:HH:mm:ss} [{logLevel.ToString()[0]}/{Name}] {message}";
        Console.WriteLine(logMessage);
    }

    /// <summary>
    /// 记录信息
    /// </summary>
    public void Info(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Log(LogLevels.Info, message);
    }

    /// <summary>
    /// 记录成功信息
    /// </summary>
    public void Success(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Log(LogLevels.Success, message);
    }

    /// <summary>
    /// 记录警告信息
    /// </summary>
    /// <param name="message"></param>
    public void Warning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Log(LogLevels.Warning, message);
    }

    /// <summary>
    /// 记录错误信息
    /// </summary>
    public void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Log(LogLevels.Error, message);
    }

    /// <summary>
    /// 记录异常
    /// </summary>
    public void Error(Exception exception)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Log(LogLevels.Error, $"{exception.GetType().Name}: {exception.Message}\n{exception.StackTrace}");
    }

    /// <summary>
    /// 记录调试信息
    /// </summary>
    public void Debug(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Log(LogLevels.Debug, message);
    }
    /// <summary>
    /// 日志事件委托
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    public delegate void LoggerEventHandler<in TEvent>(Logger logger, TEvent e);

    /// <summary>
    /// 日志记录前
    /// </summary>
    public event LoggerEventHandler<LoggerLoggingEvent>? OnLoggerLogging;
}

/// <summary>
/// 日志等级
/// </summary>
public enum LogLevels
{
    /// <summary>
    /// 调试
    /// </summary>
    Debug = 0,
        
    /// <summary>
    /// 信息
    /// </summary>
    Info = 1,
        
    /// <summary>
    /// 成功
    /// </summary>
    Success = 2,
        
    /// <summary>
    /// 警告
    /// </summary>
    Warning = 3,
        
    /// <summary>
    /// 错误
    /// </summary>
    Error = 4
}