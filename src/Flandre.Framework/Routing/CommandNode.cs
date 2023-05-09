using Flandre.Framework.Common;

namespace Flandre.Framework.Routing;

/// <summary>
/// 指令节点
/// </summary>
public sealed class CommandNode
{
    /// <summary>
    /// 以 . 分割的指令完整路径
    /// </summary>
    public string FullName { get; }

    /// <summary>
    /// 指令对象，如果该节点不包含指令则为 null
    /// </summary>
    public Command? Command { get; internal set; }

    /// <summary>
    /// 子节点
    /// </summary>
    public Dictionary<string, CommandNode> SubNodes { get; } = new();

    /// <summary>
    /// 当前节点包含指令
    /// </summary>
    public bool HasCommand => Command is not null;

    /// <summary>
    /// 当前指令节点为某个指令的别名
    /// </summary>
    public bool IsAlias { get; internal set; }

    internal CommandNode(string fullName) => FullName = fullName;

    internal Command MapCommand(Type? pluginType, string relativePath)
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

    /// <summary>
    /// 移除本身节点所含指令，并清除所有子节点
    /// </summary>
    public void Clear()
    {
        Command = null;
        IsAlias = false;
        SubNodes.Clear();
    }
}
