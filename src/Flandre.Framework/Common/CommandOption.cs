namespace Flandre.Framework.Common;

/// <summary>
/// 指令选项
/// </summary>
public sealed class CommandOption
{
    /// <summary>
    /// 选项名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 选项短名称
    /// </summary>
    public char ShortName { get; }

    /// <summary>
    /// 是否有短名称
    /// </summary>
    public bool HasShortName { get; }

    /// <summary>
    /// 选项类型
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// 选项默认值
    /// </summary>
    public object? DefaultValue { get; }

    /// <summary>
    /// 选项描述
    /// </summary>
    public string? Description { get; init; }

    internal CommandOption(string name, char shortName, Type type, object? defaultValue)
    {
        Name = name;
        ShortName = shortName;
        HasShortName = shortName != default;
        Type = type;
        DefaultValue = defaultValue;
    }
}
