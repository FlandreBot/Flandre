using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;
using Flandre.Core.Messaging;
using Flandre.Framework.Attributes;

namespace Flandre.Framework.Common;

/// <summary>
/// 表示一个指令
/// </summary>
public sealed class Command
{
    /// <summary>
    /// 指令名
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 指令完整名称（路径）
    /// </summary>
    public string FullName { get; }

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

    internal bool IsObsolete { get; set; }
    internal string? ObsoleteMessage { get; set; }
    internal string? Description { get; set; }

    private readonly CommandNode _currentNode;

    internal Command(CommandNode currentNode, Type pluginType, string name, string fullName)
    {
        _currentNode = currentNode;
        PluginType = pluginType;
        Name = name;
        FullName = fullName;
    }

    #region FluentAPI

    /// <summary>
    /// 添加别名
    /// </summary>
    /// <param name="aliasPath"></param>
    /// <returns></returns>
    public Command AddAlias(string aliasPath)
    {
        Aliases.Add(aliasPath);
        return this;
    }

    /// <summary>
    /// 添加前缀式快捷方式
    /// </summary>
    /// <param name="shortcut"></param>
    /// <returns></returns>
    public Command AddShortcut(string shortcut)
    {
        StringShortcuts.Add(shortcut);
        return this;
    }

    /// <summary>
    /// 添加正则式快捷方式
    /// </summary>
    /// <param name="shortcut"></param>
    /// <returns></returns>
    public Command AddShortcut(Regex shortcut)
    {
        RegexShortcuts.Add(shortcut);
        return this;
    }

    /// <summary>
    /// 添加指令方法
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <returns></returns>
    public Command WithAction(MethodInfo methodInfo)
    {
        InnerMethod = methodInfo;
        return this;
    }

    /// <summary>
    /// 添加子指令
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public Command AddSubCommand(string path)
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
        for (var i = 1; i < methodParams.Length; ++i)
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
