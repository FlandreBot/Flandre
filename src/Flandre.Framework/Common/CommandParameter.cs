namespace Flandre.Framework.Common;

/// <summary>
/// 指令参数
/// </summary>
public sealed class CommandParameter
{
    /// <summary>
    /// 参数名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 参数类型
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// 参数默认值
    /// </summary>
    public object? DefaultValue { get; }

    /// <summary>
    /// 参数描述
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 是否被 params 修饰
    /// </summary>
    public bool IsParamArray { get; }

    /// <summary>
    /// 是否为必须参数
    /// </summary>
    public bool IsRequired { get; }

    internal CommandParameter(string name, Type type, bool isRequired, object? defaultValue, bool isParamArray)
    {
        Name = name;
        Type = type;
        IsRequired = isRequired;
        DefaultValue = isParamArray ? Array.CreateInstance(type.GetElementType()!, 0) : defaultValue;
        IsParamArray = isParamArray;
    }
}
