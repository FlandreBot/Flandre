using Flandre.Core.Utils;

namespace Flandre.Core.Attributes;

/// <summary>
/// 指令别名特性
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class AliasAttribute : Attribute
{
    /// <summary>
    /// 指令别名
    /// </summary>
    public string Alias { get; set; }

    /// <summary>
    /// 为指令添加别名
    /// </summary>
    /// <param name="alias">指令别名</param>
    public AliasAttribute(string alias)
    {
        Alias = CommandUtils.NormalizeCommandDefinition(alias);
    }
}