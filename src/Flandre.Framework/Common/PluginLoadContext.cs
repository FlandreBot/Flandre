using System.ComponentModel;
using System.Reflection;
using Flandre.Framework.Attributes;
using Microsoft.Extensions.Logging;

namespace Flandre.Framework.Common;

/// <summary>
/// 读取插件的上下文
/// </summary>
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

    /// <summary>
    /// 添加指令
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
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
                    // if (!param.HasDefaultValue)
                    // {
                    //     _logger.LogWarning("指令方法 {PluginType}.{MethodName} 中选项参数 {OptionName} 不具有默认值, 将忽略这个选项。",
                    //         _pluginType, method.Name, param.Name);
                    //     continue;
                    // }
                    var paramType = param.ParameterType;

                    options.Add(new CommandOption(param.Name!, optAttr.ShortName, paramType,
                        param.HasDefaultValue // 如果 option 定义了默认值
                            ? param.DefaultValue! // 则使用定义的默认值
                            // 否则使用 default(T)
                            : param.ParameterType.IsValueType
                                ? Activator.CreateInstance(paramType)
                                : null) { Description = description });
                    continue;
                }

                // parameter
                parameters.Add(new CommandParameter(param.Name!, param.ParameterType,
                    param.HasDefaultValue ? param.DefaultValue : null) { Description = description });
            }

            var cmd = AddCommand(cmdAttr.FullName).WithAction(method);

            foreach (var alias in cmdAttr.Aliases)
                cmd.AddAlias(alias);

            cmd.Parameters = parameters;
            cmd.Options = options;

            cmd.Shortcuts.AddRange(method.GetCustomAttributes<StringShortcutAttribute>()
                .Select(attr => new StringShortcut(attr)));
            cmd.Shortcuts.AddRange(method.GetCustomAttributes<RegexShortcutAttribute>()
                .Select(attr => new RegexShortcut(attr)));

            // cmd.StringShortcuts = method.GetCustomAttributes<StringShortcutAttribute>()
            //     .Select(attr => attr.StringShortcut).ToList();
            // cmd.RegexShortcuts = method.GetCustomAttributes<RegexShortcutAttribute>()
            //     .Select(attr => attr.RegexShortcut).ToList();

            var obsoleteAttr = method.GetCustomAttribute<ObsoleteAttribute>();
            cmd.IsObsolete = obsoleteAttr is not null;
            cmd.ObsoleteMessage = obsoleteAttr?.Message;
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
                    : currentNode.SubNodes[segments[i]] = new CommandNode(
                        string.Join('.', segments[..(i + 1)]));

                if (i == segments.Length - 1)
                {
                    currentNode.Command = cmd;
                    currentNode.IsAlias = true;
                }
            }
        }
    }

    internal void LoadCommandShortcuts(Dictionary<StringShortcut, Command> stringShortcuts,
        Dictionary<RegexShortcut, Command> regexShortcuts)
    {
        void LoadNodeShortcuts(CommandNode node)
        {
            if (node.HasCommand)
            {
                foreach (var shortcut in node.Command!.Shortcuts)
                {
                    switch (shortcut)
                    {
                        case StringShortcut strShortcut:
                            stringShortcuts[strShortcut] = node.Command;
                            break;

                        case RegexShortcut regShortcut:
                            regexShortcuts[regShortcut] = node.Command;
                            break;
                    }
                }
                // foreach (var strShortcut in node.Command!.StringShortcuts)
                //     stringShortcuts[strShortcut] = node.Command;
                // foreach (var regexShortcut in node.Command!.RegexShortcuts)
                //     regexShortcuts[regexShortcut] = node.Command;
            }

            foreach (var (_, subNode) in node.SubNodes)
                LoadNodeShortcuts(subNode);
        }

        LoadNodeShortcuts(_rootNode);
    }

    #endregion
}
