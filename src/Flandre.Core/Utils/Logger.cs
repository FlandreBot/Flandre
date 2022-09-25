namespace Flandre.Core.Utils;

/// <summary>
/// 日志记录器
/// </summary>
public class Logger
{
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
    
    private static void Log(string message)
    {
        var logMessage = $"{DateTime.Now:HH:mm:ss} {message}";
        Console.WriteLine(logMessage);
    }

    /// <summary>
    /// 记录信息
    /// </summary>
    public void Info(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Log($"[I/{Name}] {message}");
    }

    /// <summary>
    /// 记录成功信息
    /// </summary>
    public void Success(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Log($"[S/{Name}] {message}");
    }

    /// <summary>
    /// 记录警告信息
    /// </summary>
    /// <param name="message"></param>
    public void Warning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Log($"[W/{Name}] {message}");
    }

    /// <summary>
    /// 记录错误信息
    /// </summary>
    public void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Log($"[E/{Name}] {message}");
    }

    /// <summary>
    /// 记录异常
    /// </summary>
    public void Error(Exception exception)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Log($"[E/{Name}] {exception.GetType().Name}: {exception.Message}\n{exception.StackTrace}");
    }

    /// <summary>
    /// 记录调试信息
    /// </summary>
    public void Debug(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Log($"[D/{Name}] {message}");
    }
}