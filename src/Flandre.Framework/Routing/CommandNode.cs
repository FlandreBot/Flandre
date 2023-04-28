using Flandre.Framework.Common;

namespace Flandre.Framework.Routing;

internal sealed class CommandNode
{
    public string FullName { get; }

    public Command? Command { get; internal set; }

    public Dictionary<string, CommandNode> SubNodes { get; } = new();

    public bool HasCommand => Command is not null;

    public bool IsAlias { get; internal set; }

    public CommandNode(string fullName) => FullName = fullName;

    public Command MapCommand(Type? pluginType, string relativePath)
    {
        var segments = relativePath.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var currentNode = this;

        for (var i = 0; i < segments.Length; i++)
        {
            currentNode = currentNode.SubNodes.TryGetValue(segments[i], out var nextNode)
                ? nextNode
                : currentNode.SubNodes[segments[i]] = new CommandNode(
                    string.Join('.', segments[..(i + 1)]));
        }

        var finalName = segments[^1];
        var command = new Command(currentNode, pluginType, finalName, currentNode.FullName);
        currentNode.Command = command;
        return command;
    }

    public CommandNode? FindSubNode(string relativePath)
    {
        var node = this;
        foreach (var name in relativePath.Split('.',
                     StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            if (node.SubNodes.Keys.FirstOrDefault(k => k.Equals(name, StringComparison.OrdinalIgnoreCase)) is { } key)
                node = node.SubNodes[key];
            else
                return null;

        return node;
    }

    public int CountCommands()
    {
        var count = 0;

        void CountNodeCommands(CommandNode node)
        {
            if (node is { HasCommand: true, IsAlias: false })
                count++;
            foreach (var (_, subNode) in node.SubNodes)
                CountNodeCommands(subNode);
        }

        CountNodeCommands(this);
        return count;
    }
}
