namespace Flandre.Framework.Common;

internal sealed class CommandNode
{
    public Command? Command { get; internal set; }

    public Dictionary<string, CommandNode> SubNodes { get; } = new();

    public bool HasCommand => Command is not null;

    public Command AddCommand(Type pluginType, string path)
    {
        var node = this;
        var segments = path.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        node = segments.Aggregate(node,
            (current, name) => current.SubNodes.TryGetValue(name, out var nextNode)
                ? nextNode
                : current.SubNodes[name] = new CommandNode());

        var finalName = segments[^1];
        var command = new Command(node, pluginType, finalName);
        node.Command = command;
        return command;
    }

    public CommandNode? GetNodeByPath(string path)
    {
        var node = this;
        foreach (var name in path.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            if (node.SubNodes.TryGetValue(name, out var nextNode))
                node = nextNode;
            else
                return null;

        return node;
    }

    public int CountCommands()
    {
        var count = 0;

        void CountNodeCommands(CommandNode node)
        {
            if (node.HasCommand)
                count++;
            foreach (var (_, subNode) in node.SubNodes)
                CountNodeCommands(subNode);
        }

        CountNodeCommands(this);
        return count;
    }
}
