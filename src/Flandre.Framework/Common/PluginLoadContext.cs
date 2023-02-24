using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using Flandre.Core.Messaging;
using Flandre.Framework.Attributes;
using Microsoft.Extensions.Logging;

namespace Flandre.Framework.Common;

public sealed class PluginLoadContext
{
    private readonly CommandNode _rootNode;
    private readonly Type _pluginType;
    private readonly ILogger<PluginLoadContext> _logger;

    internal PluginLoadContext(CommandNode rootNode, Type pluginType, ILogger<PluginLoadContext> logger)
    {
        _rootNode = rootNode;
        _pluginType = pluginType;
        _logger = logger;
    }

    public Command AddCommand(string path)
    {
        return _rootNode.AddCommand(_pluginType, path);
    }

    #region Internal Processing

    internal void LoadFromAttributes()
    {
        foreach (var method in _pluginType.GetMethods())
        {
            _logger.LogTrace("正在加载方法 {PluginType}.{MethodName}", _pluginType, method.Name);
            var cmdAttr = method.GetCustomAttribute<CommandAttribute>();
            if (cmdAttr is null)
                continue;

            // 方法类型不对的
            if (!(method.ReturnType.IsAssignableTo(typeof(MessageContent))
                  || method.ReturnType.IsAssignableTo(typeof(Task<MessageContent>))
                  || method.ReturnType.IsAssignableTo(typeof(ValueTask<MessageContent>))))
            {
                _logger.LogWarning("命令方法 {PluginType}.{MethodName} 的返回类型不是 MessageContent, 将忽略这个命令。",
                    _pluginType, method.Name);
                continue;
            }

            var options = new List<CommandOption>();
            var parameters = new List<CommandParameter>();

            foreach (var param in method.GetParameters())
            {
                if (param.ParameterType.IsAssignableFrom(typeof(CommandContext)))
                    continue;

                var description = param.GetCustomAttribute<DescriptionAttribute>()?.Description;

                if (param.GetCustomAttribute<OptionAttribute>() is { } optAttr)
                {
                    // option
                    if (!param.HasDefaultValue)
                    {
                        _logger.LogWarning("命令方法 {PluginType}.{MethodName} 中选项参数 {OptionName} 不具有默认值, 将忽略这个命令。",
                            _pluginType, method.Name, param.Name);
                        continue;
                    }

                    options.Add(new CommandOption(param.Name!, optAttr.ShortName, param.DefaultValue!)
                    {
                        Description = description
                    });
                    continue;
                }

                // parameter
                parameters.Add(new CommandParameter(param.Name!, param.ParameterType,
                    param.HasDefaultValue ? param.DefaultValue : null) { Description = description });
            }

            var cmd = AddCommand(cmdAttr.Path).WithAction(method);

            foreach (var alias in cmdAttr.Alias)
                cmd.AddAlias(alias);

            cmd.Parameters = parameters;
            cmd.Options = options;
            cmd.StringShortcuts = method.GetCustomAttributes<StringShortcutAttribute>()
                .Select(attr => attr.StringShortcut).ToList();
            cmd.RegexShortcuts = method.GetCustomAttributes<RegexShortcutAttribute>()
                .Select(attr => attr.RegexShortcut).ToList();

            cmd.IsObsoleted = method.GetCustomAttribute<ObsoleteAttribute>() is not null;
            cmd.Description = method.GetCustomAttribute<DescriptionAttribute>()?.Description;
        }
    }

    internal void LoadCommandAliases()
    {
        var toBeAdded = new Dictionary<string, Command>();

        void LoadNodeAliases(CommandNode node)
        {
            if (node.HasCommand)
                foreach (var alias in node.Command!.Aliases)
                    toBeAdded[alias] = node.Command;

            foreach (var (_, subNode) in node.SubNodes)
                LoadNodeAliases(subNode);
        }

        LoadNodeAliases(_rootNode);

        foreach (var (alias, cmd) in toBeAdded)
        {
            var currentNode = _rootNode;
            var segments = alias.Split('.',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            for (var i = 0; i < segments.Length; i++)
            {
                currentNode = currentNode.SubNodes.TryGetValue(segments[i], out var nextNode)
                    ? nextNode
                    : currentNode.SubNodes[segments[i]] = new CommandNode();

                if (i == segments.Length - 1)
                {
                    currentNode.Command = cmd;
                    currentNode.IsAlias = true;
                }
            }
        }
    }

    internal void LoadCommandShortcuts(Dictionary<string, Command> stringShortcuts,
        Dictionary<Regex, Command> regexShortcuts)
    {
        void LoadNodeShortcuts(CommandNode node)
        {
            if (node.HasCommand)
            {
                foreach (var strShortcut in node.Command!.StringShortcuts)
                    stringShortcuts[strShortcut] = node.Command;
                foreach (var regexShortcut in node.Command!.RegexShortcuts)
                    regexShortcuts[regexShortcut] = node.Command;
            }

            foreach (var (_, subNode) in node.SubNodes)
                LoadNodeShortcuts(subNode);
        }

        LoadNodeShortcuts(_rootNode);
    }

    #endregion
}
