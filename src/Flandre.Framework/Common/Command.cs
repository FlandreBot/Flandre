using System.Reflection;
using System.Text.RegularExpressions;
using Flandre.Core.Messaging;
using Flandre.Framework.Attributes;
using Microsoft.Extensions.Logging;

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

    internal List<CommandShortcut> Shortcuts { get; } = new();

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
    /// 添加字符串匹配快捷方式
    /// </summary>
    public Command AddShortcut(string shortcut, string target, bool allowArguments = false)
    {
        Shortcuts.Add(new StringShortcut(shortcut, target, allowArguments));
        return this;
    }

    /// <summary>
    /// 添加正则式快捷方式
    /// </summary>
    public Command AddShortcut(Regex shortcut, string target)
    {
        Shortcuts.Add(new RegexShortcut(shortcut, target));
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

    internal async Task<MessageContent?> InvokeAsync(Plugin plugin, CommandContext ctx,
        CommandParser.CommandParseResult parsed, ILogger logger)
    {
        if (InnerMethod is null)
            return null;

        var args = new List<object?>();
        var parsedArgIndex = 0;
        var methodParams = InnerMethod.GetParameters();
        foreach (var param in methodParams)
        {
            if (param.ParameterType != typeof(object) &&
                param.ParameterType.IsAssignableFrom(typeof(CommandContext)))
            {
                args.Add(ctx);
                continue;
            }

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

        // void MethodReturnTypeNotResolved()
        // {
        //     logger.LogWarning("无法将指令方法 {PluginType}.{MethodName} 的返回类型转换为可解析的返回类型。",
        //         plugin.GetType().Name, InnerMethod?.Name);
        // }

        var cmdResult = InnerMethod?.Invoke(plugin, args.ToArray());
        var content = cmdResult switch
        {
            Task<MessageContent?> task => await task,
            ValueTask<MessageContent?> valueTask => await valueTask,
            MessageContent msgContent => msgContent,

            string str => (MessageContent)str,
            MessageBuilder msgBuilder => (MessageContent)msgBuilder,

            _ => cmdResult?.ToString() is { } val ? (MessageContent)val : null
        };

        return content;
    }
}
