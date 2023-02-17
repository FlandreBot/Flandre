using System.Reflection;
using System.Text.RegularExpressions;
using Flandre.Core.Messaging;
using Flandre.Framework.Attributes;

namespace Flandre.Framework.Common;

public sealed class Command
{
    public string Path { get; }
    public string Name => Path.Split('.')[^1];

    internal List<CommandParameter> Parameters { get; init; } = new();
    internal List<CommandOption> Options { get; init; } = new();

    /// <summary>
    /// Contains alias command path
    /// </summary>
    internal List<string> Aliases { get; } = new();

    internal List<string> StringShortcuts { get; } = new();
    internal List<Regex> RegexShortcuts { get; } = new();

    internal MethodInfo? InnerMethod { get; private set; }
    internal Type PluginType { get; set; }

    internal bool IsObsoleted { get; set; }
    internal string Description { get; set; }

    private readonly CommandNode _currentNode;

    internal Command(CommandNode currentNode, Type pluginType, string path)
    {
        _currentNode = currentNode;
        PluginType = pluginType;
        Path = path;
    }

    #region FluentAPI

    public Command AddParameter<TValue>(string name)
    {
        Parameters.Add(new CommandParameter(name, typeof(TValue), null));
        return this;
    }

    public Command AddParameter<TValue>(string name, TValue defaultValue)
    {
        Parameters.Add(new CommandParameter(name, typeof(TValue), defaultValue));
        return this;
    }

    public Command AddOption<TValue>(string name, char shortName, TValue defaultValue)
    {
        Options.Add(new CommandOption(name, shortName, defaultValue ?? default(TValue)!));
        return this;
    }

    public Command AddOption<TValue>(string name, TValue defaultValue)
    {
        Options.Add(new CommandOption(name, defaultValue ?? default(TValue)!));
        return this;
    }

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

    /// <summary>
    /// Auto gets information from method definition.
    /// </summary>
    public Command EnrichFromAction()
    {
        if (InnerMethod is not null)
        {
            var parameters = InnerMethod.GetParameters();
            foreach (var param in parameters)
            {
                // TODO
            }
        }

        return this;
    }

    public Command AddSubcommand(string path)
    {
        return _currentNode.AddCommand(PluginType, path);
    }

    #endregion

    internal MessageContent? Invoke(Plugin plugin, CommandContext ctx, CommandParser.CommandParseResult parsed)
    {
        if (InnerMethod is null) return null;

        var args = new List<object> { ctx };
        var index = 0;
        foreach (var param in InnerMethod.GetParameters())
        {
            if (index > parsed.ParsedArguments.Count - 1)
                throw new CommandInvokeException("Too many arguments requested.");
            var attr = param.GetCustomAttribute<OptionAttribute>();
            if (attr is null)
            {
                // argument
                args.Add(parsed.ParsedArguments[index]);
                index++;
            }
            else
            {
                if (!parsed.ParsedOptions.TryGetValue(attr.Name, out var obj))
                    throw new CommandInvokeException($"No such option named {attr.Name}.");
                args.Add(obj);
            }
        }

        var cmdResult = InnerMethod?.Invoke(plugin, args.ToArray());
        var content = cmdResult as MessageContent ??
                      (cmdResult as Task<MessageContent>)?.GetAwaiter().GetResult() ?? null;

        return content;
    }
}
