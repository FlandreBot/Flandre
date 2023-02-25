namespace Flandre.Framework.Attributes;

/// <summary>
/// 选项
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class OptionAttribute : Attribute
{
    /// <summary>
    /// 短名称
    /// </summary>
    /// <remarks>长名称取决于参数名</remarks>
    public char ShortName { get; init; }
}
