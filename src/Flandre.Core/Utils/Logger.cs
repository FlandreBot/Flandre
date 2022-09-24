namespace Flandre.Core.Utils;

public class Logger
{
    public string Name { get; set; }

    public Logger(string name)
    {
        Name = name;
    }

    private static void Log(string message)
    {
        var logMessage = $"{DateTime.Now:HH:mm:ss} {message}";
        Console.WriteLine(logMessage);
    }

    public void Info(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Log($"[I/{Name}] {message}");
    }

    public void Success(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Log($"[S/{Name}] {message}");
    }

    public void Warning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Log($"[W/{Name}] {message}");
    }

    public void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Log($"[E/{Name}] {message}");
    }

    public void Error(Exception exception)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Log($"[E/{Name}] {exception.GetType().Name}: {exception.Message}\n{exception.StackTrace}");
    }

    public void Debug(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Log($"[D/{Name}] {message}");
    }
}