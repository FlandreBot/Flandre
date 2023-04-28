using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using Flandre.Core.Messaging;
using Flandre.Framework.Attributes;
using Flandre.Framework.Routing;
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

    /// <summary>
    /// 调用指令方法的目标。如果不为 null 代表该方法是一个委托，否则为指令方法。
    /// </summary>
    internal object? InvokeTarget { get; private set; }

    /// <summary>
    /// 调用指令方法的目标。如果为 null 代表该方法是一个委托，否则为指令方法。
    /// </summary>
    internal Type? PluginType { get; }

    internal MethodInfo? InnerMethod { get; private set; }

    internal bool IsObsolete { get; set; }
    internal string? ObsoleteMessage { get; set; }
    internal string? Description { get; set; }

    private readonly CommandNode _currentNode;

    internal Command(CommandNode currentNode, Type? pluginType, string name, string fullName)
    {
        _currentNode = currentNode;
        PluginType = pluginType;
        Name = name;
        FullName = fullName;
    }

    private void InferFromMethod(MethodBase method)
    {
        Options.Clear();
        Parameters.Clear();

        foreach (var param in method.GetParameters())
        {
            if (param.ParameterType.IsAssignableFrom(typeof(CommandContext)))
                continue;

            var description = param.GetCustomAttribute<DescriptionAttribute>()?.Description;

            if (param.GetCustomAttribute<OptionAttribute>() is { } optAttr)
            {
                // option
                var paramType = param.ParameterType;

                Options.Add(new CommandOption(param.Name!, optAttr.ShortName, paramType,
                    param.HasDefaultValue // 如果 option 定义了默认值
                        ? param.DefaultValue! // 则使用定义的默认值
                        // 否则使用 default(T)
                        : param.ParameterType.IsValueType
                            ? Activator.CreateInstance(paramType)
                            : null) { Description = description });
                continue;
            }

            // parameter
            Parameters.Add(new CommandParameter(param.Name!, param.ParameterType,
                param.HasDefaultValue ? param.DefaultValue : null,
                param.ParameterType.IsArray && param.GetCustomAttribute<ParamArrayAttribute>() is not null)
            {
                Description = description
            });
        }
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

    internal Command WithAction(MethodInfo methodInfo, object? target = null)
    {
        InnerMethod = methodInfo;
        InvokeTarget = target;
        InferFromMethod(InnerMethod);
        return this;
    }

    /// <summary>
    /// 添加指令方法
    /// </summary>
    /// <param name="commandDelegate"></param>
    /// <returns></returns>
    public Command WithAction(Delegate commandDelegate)
    {
        return WithAction(commandDelegate.Method, commandDelegate.Target);
    }

    /// <summary>
    /// 添加子指令
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public Command AddSubCommand(string path)
    {
        return _currentNode.MapCommand(PluginType, path);
    }

    #endregion

    internal async Task<MessageContent?> InvokeAsync(
        Plugin? plugin, CommandContext ctx,
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

        var target = plugin ?? InvokeTarget;
        if (target is null)
        {
            logger.LogError("指令 {CommandFullName} 调用失败，非插件指令需要有调用目标。", FullName);
            return null;
        }

        var cmdResult = InnerMethod?.Invoke(target, args.ToArray());
        var content = cmdResult switch
        {
            MessageContent mc => mc,
            Task<MessageContent?> mcTask => await mcTask,
            ValueTask<MessageContent?> mcValueTask => await mcValueTask,

            MessageBuilder mb => mb.Build(),
            Task<MessageBuilder> mbTask => (MessageContent)await mbTask,
            ValueTask<MessageBuilder> mbValueTask => (MessageContent)await mbValueTask,

            Task<string> strTask => (MessageContent)await strTask,
            ValueTask<string> strValueTask => (MessageContent)await strValueTask,

            _ => cmdResult?.ToString() is { } val ? (MessageContent)val : null
        };

        return content;
    }
}
