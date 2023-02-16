namespace Flandre.Framework.Attributes;

/// <summary>
/// 指令定义
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class CommandAttribute : Attribute
{
    /// <summary>
    /// 指令路径
    /// </summary>
    public string Path { get; }

    public CommandAttribute(string path)
    {
        Path = path;
    }
}