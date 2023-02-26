using Flandre.Core.Models;

namespace Flandre.Framework.Attributes;

/// <summary>
/// 指令
/// </summary>
/// <remarks>被这条特性指定的方法，会自动作为指令被机器人加载</remarks>
[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : Attribute
{
    /// <summary>
    /// 构造特性实例
    /// </summary>
    /// <param name="fullName"></param>
    /// <param name="aliases"></param>
    public CommandAttribute(string fullName, params string[] aliases)
    {
        FullName = fullName;
        Aliases = aliases;
    }

    /// <summary>
    /// 指令的全名
    /// </summary>
    public string FullName { get; }

    /// <summary>
    /// 指令别名（全名）
    /// </summary>
    public string[] Aliases { get; }

    /// <summary>
    /// 能触发该指令的用户身份
    /// </summary>
    /// <remarks>默认值为 <see cref="UserRole.Member"/></remarks>
    public UserRole Role { get; init; } = UserRole.Member;

    /// <summary>
    /// 是否允许忽略大小写
    /// </summary>
    /// <remarks>默认值为 <see langword="true"/></remarks>
    public bool IgnoreCase { get; init; } = true;
}
