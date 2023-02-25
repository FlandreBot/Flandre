using System.Text.RegularExpressions;

namespace Flandre.Framework.Attributes;

/// <summary>
/// 为指令添加前缀式快捷方式
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class StringShortcutAttribute : Attribute
{
    /// <summary>
    /// 前缀式快捷方式
    /// </summary>
    public string StringShortcut { get; }

    /// <summary>
    /// 构造特性实例
    /// </summary>
    /// <param name="shortcut">前缀式快捷方式</param>
    public StringShortcutAttribute(string shortcut) => StringShortcut = shortcut;
}

/// <summary>
/// 为指令添加正则式快捷方式
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RegexShortcutAttribute : Attribute
{
    /// <summary>
    /// 正则式快捷方式
    /// </summary>
    public Regex RegexShortcut { get; }

    /// <summary>
    /// 构造特性实例
    /// </summary>
    /// <param name="pattern">正则式快捷方式</param>
    public RegexShortcutAttribute(string pattern) => RegexShortcut = new Regex(pattern);

    /// <summary>
    /// 构造特性实例
    /// </summary>
    /// <param name="regex">正则式快捷方式</param>
    public RegexShortcutAttribute(Regex regex) => RegexShortcut = regex;
}
