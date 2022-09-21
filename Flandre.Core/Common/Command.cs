using System.Reflection;

namespace Flandre.Core.Common;

public class Command
{
    public CommandAttribute CommandInfo { get; set; }

    public MethodInfo InnerMethod { get; set; }

    public Command(CommandAttribute info, MethodInfo innerMethod)
    {
        CommandInfo = info;
        InnerMethod = innerMethod;
    }
}

internal static class CommandExtension
{
    internal static void ParseCommand(this Plugin plugin, string raw)
    {
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : Attribute
{
    public string Pattern { get; }

    public CommandAttribute(string pattern)
    {
        Pattern = pattern;
    }
}