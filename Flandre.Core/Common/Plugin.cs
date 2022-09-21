using System.Reflection;
using Flandre.Core.Messaging;
using Flandre.Core.Utils;

namespace Flandre.Core.Common;

public abstract class Plugin : IModule
{
    public Logger Logger { get; }

    public List<Command> Commands { get; } = new();

    public PluginAttribute PluginInfo { get; }

    public Plugin()
    {
        var type = GetType();
        PluginInfo = type.GetCustomAttribute<PluginAttribute>() ?? new PluginAttribute(type.Name);
        Logger = new Logger(PluginInfo.Name);

        foreach (var method in type.GetMethods())
        {
            var attr = method.GetCustomAttribute<CommandAttribute>();
            if (attr is not null)
                Commands.Add(new Command(attr, method));
        }
    }

    internal MessageContent? OnCommandParsing(Context ctx)
    {
        var commandStr = ctx.Message.GetText();
        if (string.IsNullOrWhiteSpace(commandStr)) return null;

        foreach (var command in Commands)
        {
            var basePattern = ctx.App.Config.CommandPrefix +
                              (PluginInfo.BaseCommand + ' ' +
                               command.CommandInfo.Pattern.Split(" ")[0]).TrimStart();

            var startsWithFlag = commandStr.StartsWith(basePattern);

            if (!startsWithFlag)
                continue;

            var result = command.InnerMethod.Invoke(
                this, new object[] { ctx }[..command.InnerMethod.GetParameters().Length]);
            var content = result as MessageContent ?? (result as Task<MessageContent>)?.Result ?? null;
            return content;
        }

        return null;
    }

    internal ParsedArgs ParseCommand()
    {
        var args = new ParsedArgs();

        return args;
    }

    public virtual void OnMessageReceived(Context ctx)
    {
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class PluginAttribute : Attribute
{
    public string Name { get; }

    public string? BaseCommand { get; set; }

    public PluginAttribute(string name)
    {
        Name = name;
    }
}