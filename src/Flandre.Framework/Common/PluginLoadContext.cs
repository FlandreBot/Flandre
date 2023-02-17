using System.Text.RegularExpressions;
using Flandre.Core.Messaging;
using Flandre.Framework.Utils;

namespace Flandre.Framework.Common;

public sealed class PluginLoadContext
{
    internal readonly CommandNode RootCommandNode = new(true);
    internal readonly Dictionary<string, Command> StringShortcuts = new();
    internal readonly Dictionary<Regex, Command> RegexShortcuts = new();

    internal Type CurrentPluginType = null!;

    internal int CommandCount => CountCommands();
    internal int AliasCount { get; private set; }

    public Command AddCommand(string path)
    {
        return RootCommandNode.AddCommand(CurrentPluginType, path);
    }

    internal void LoadCommandAliases()
    {
        void LoadNodeAliases(CommandNode node)
        {
            if (node.HasCommand)
            {
                foreach (var alias in node.Command!.Aliases)
                {
                    var currentNode = RootCommandNode;
                    var segments = alias.Split('.',
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    for (var i = 0; i < segments.Length; i++)
                    {
                        currentNode = currentNode.Subcommands.TryGetValue(segments[i], out var nextNode)
                            ? nextNode
                            : currentNode.Subcommands[segments[i]] = new CommandNode(false);

                        // now current node is the subcommand's node

                        if (i == segments.Length - 1)
                            currentNode.Command = node.Command;
                    }
                }

                AliasCount++;
            }

            foreach (var (_, subNode) in node.Subcommands) LoadNodeAliases(subNode);
        }

        LoadNodeAliases(RootCommandNode);
    }

    internal void LoadCommandShortcuts()
    {
        void LoadNodeShortcuts(CommandNode node)
        {
            if (node.HasCommand)
            {
                foreach (var strShortcut in node.Command!.StringShortcuts)
                    StringShortcuts[strShortcut] = node.Command;
                foreach (var regexShortcut in node.Command!.RegexShortcuts)
                    RegexShortcuts[regexShortcut] = node.Command;
            }

            foreach (var (_, subNode) in node.Subcommands) LoadNodeShortcuts(subNode);
        }

        LoadNodeShortcuts(RootCommandNode);
    }

    private int CountCommands()
    {
        var count = 0;

        void CountNodeCommands(CommandNode node)
        {
            if (node.HasCommand) count++;
            foreach (var (_, subNode) in node.Subcommands)
                CountNodeCommands(subNode);
        }

        CountNodeCommands(RootCommandNode);
        return count;
    }

    public void AddCommandFromAttributes()
    {
        CmdParser.Parse<MessageContent>(RootCommandNode, CurrentPluginType);
    }
}
