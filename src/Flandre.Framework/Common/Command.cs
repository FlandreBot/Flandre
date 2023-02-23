using System.Reflection;
using System.Text.RegularExpressions;
using Flandre.Core.Messaging;
using Flandre.Framework.Attributes;

namespace Flandre.Framework.Common;

public sealed class Command
{
    public string Name { get; }

    internal List<CommandParameter> Parameters { get; set; } = new();
    internal List<CommandOption> Options { get; set; } = new();

    /// <summary>
    /// Contains alias command path
    /// </summary>
    internal List<string> Aliases { get; } = new();

    internal List<string> StringShortcuts { get; set; } = new();
    internal List<Regex> RegexShortcuts { get; set; } = new();

    internal MethodInfo? InnerMethod { get; private set; }
    internal Type PluginType { get; }

    internal bool IsObsoleted { get; set; }
    internal string? Description { get; set; }

    private readonly CommandNode _currentNode;

    internal Command(CommandNode currentNode, Type pluginType, string name)
    {
        _currentNode = currentNode;
        PluginType = pluginType;
        Name = name;
    }

    #region FluentAPI

    public Command AddAlias(string aliasPath)
    {
        Aliases.Add(aliasPath);
        return this;
    }

    public Command AddShortcut(string shortcut)
    {
        StringShortcuts.Add(shortcut);
        return this;
    }

    public Command AddShortcut(Regex shortcut)
    {
        RegexShortcuts.Add(shortcut);
        return this;
    }

    public Command WithAction(MethodInfo methodInfo)
    {
        InnerMethod = methodInfo;
        return this;
    }

    public Command AddSubcommand(string path)
    {
        return _currentNode.AddCommand(PluginType, path);
    }

    #endregion

    internal MessageContent? Invoke(Plugin plugin, CommandContext ctx, CommandParser.CommandParseResult parsed)
    {
        if (InnerMethod is null)
            return null;

        var args = new List<object> { ctx };
        var parsedArgIndex = 0;
        var methodParams = InnerMethod.GetParameters();
        for (var i = 1; i < methodParams.Length; i++)
        {
            var param = methodParams[i];
            if (parsedArgIndex > parsed.ParsedArguments.Count)
                throw new CommandInvokeException("Too many arguments requested.");
            var attr = param.GetCustomAttribute<OptionAttribute>();
            if (attr is null)
            {
                // argument
                args.Add(parsed.ParsedArguments[parsedArgIndex]);
                parsedArgIndex++;
            }
            else
            {
                // option
                if (param.Name is null)
                    throw new CommandInvokeException("Option parameter must have a name.");
                if (!parsed.ParsedOptions.TryGetValue(param.Name, out var obj))
                    throw new CommandInvokeException($"No such option named {param.Name}.");
                args.Add(obj);
            }
        }

        var cmdResult = InnerMethod?.Invoke(plugin, args.ToArray());

        var content = cmdResult is ValueTask<MessageContent?> valueTask
            ? valueTask.GetAwaiter().GetResult()
            : cmdResult as MessageContent ??
              (cmdResult as Task<MessageContent?>)?.GetAwaiter().GetResult();
        return content;
    }
}
