namespace Flandre.Framework.Common;

internal sealed class CommandNode
{
    public Command? Command { get; internal set; }

    public Dictionary<string, CommandNode> Subcommands { get; } = new();

    public bool HasCommand => Command is not null;

    public bool IsRoot { get; }

    public CommandNode(bool isRoot)
    {
        IsRoot = isRoot;
    }

    public Command AddCommand(Type pluginType, string path)
    {
        var node = this;
        var segments = path.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (var name in segments)
            if (node.Subcommands.TryGetValue(name, out var nextNode))
                node = nextNode;
            else node = node.Subcommands[name] = new CommandNode(false);

        var finalName = segments[^1];
        var command = new Command(node, pluginType, finalName);
        node.Subcommands[finalName].Command = command;
        return command;
    }

    public CommandNode? GetNodeByPath(string path)
    {
        var node = this;
        foreach (var name in path.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            if (node.Subcommands.TryGetValue(name, out var nextNode))
                node = nextNode;
            else return null;

        return node;
    }
}