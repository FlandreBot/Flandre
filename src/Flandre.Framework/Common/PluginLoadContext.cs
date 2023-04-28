using System.ComponentModel;
using System.Reflection;
using Flandre.Framework.Attributes;
using Flandre.Framework.Routing;
using Flandre.Framework.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Flandre.Framework.Common;

/// <summary>
/// 读取指令的上下文
/// </summary>
internal sealed class PluginLoadContext
{
    private readonly CommandService _cmdService;
    private readonly Type _pluginType;
    private readonly ILogger<PluginLoadContext> _logger;

    /// <param name="pluginType">如果为 null，代表将要加载一个闭包</param>
    /// <param name="services"></param>
    internal PluginLoadContext(Type pluginType, IServiceProvider services)
    {
        _pluginType = pluginType;
        _cmdService = services.GetRequiredService<CommandService>();
        _logger = services.GetRequiredService<ILogger<PluginLoadContext>>();
    }

    /// <summary>
    /// 添加指令
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public Command MapCommand(string path)
    {
        return _cmdService.RootCommandNode.MapCommand(_pluginType, path);
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

            var cmd = MapCommand(cmdAttr.FullName ?? method.Name).WithAction(method);

            foreach (var alias in cmdAttr.Aliases)
                cmd.AddAlias(alias);

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

        LoadNodeAliases(_cmdService.RootCommandNode);

        foreach (var (alias, cmd) in toBeAdded)
        {
            var currentNode = _cmdService.RootCommandNode;
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

    internal void LoadCommandShortcuts()
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
                            _cmdService.StringShortcuts[strShortcut] = node.Command;
                            break;

                        case RegexShortcut regShortcut:
                            _cmdService.RegexShortcuts[regShortcut] = node.Command;
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

        LoadNodeShortcuts(_cmdService.RootCommandNode);
    }

    #endregion
}
