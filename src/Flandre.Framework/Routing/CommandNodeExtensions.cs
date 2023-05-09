namespace Flandre.Framework.Routing;

/// <summary>
/// 
/// </summary>
public static class CommandNodeExtensions
{
    /// <summary>
    /// 寻找子节点
    /// </summary>
    /// <param name="node"></param>
    /// <param name="relativePath"></param>
    /// <returns></returns>
    public static CommandNode? FindSubNode(this CommandNode node, string relativePath)
    {
        foreach (var name in relativePath.Split('.',
                     StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            if (node.SubNodes.Keys.FirstOrDefault(k => k.Equals(name, StringComparison.OrdinalIgnoreCase)) is { } key)
                node = node.SubNodes[key];
            else
                return null;

        return node;
    }

    public static int CountCommands(this CommandNode node)
    {
        var count = 0;

        void CountNodeCommands(CommandNode nowNode)
        {
            if (nowNode is { HasCommand: true, IsAlias: false })
                count++;
            foreach (var (_, subNode) in nowNode.SubNodes)
                CountNodeCommands(subNode);
        }

        CountNodeCommands(node);
        return count;
    }
}
