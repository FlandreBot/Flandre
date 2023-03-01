using System.Text.RegularExpressions;

namespace Flandre.Framework.Attributes;

/// <summary>
/// 为指令添加前缀式快捷方式
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class StringShortcutAttribute : Attribute
{
    /// <summary>
    /// 字符串匹配快捷方式
    /// </summary>
    public string StringShortcut { get; }

    /// <summary>
    /// 目标指令文本
    /// </summary>
    public string Target { get; }

    /// <summary>
    /// 允许附加参数
    /// </summary>
    public bool AllowArguments { get; init; }

    /// <summary>
    /// 构造特性实例
    /// </summary>
    /// <param name="shortcut">字符串匹配快捷方式</param>
    /// <param name="target">目标指令文本</param>
    public StringShortcutAttribute(string shortcut, string target)
    {
        StringShortcut = shortcut;
        Target = target;
    }

    /// <summary>
    /// 构造特性实例
    /// </summary>
    /// <param name="shortcut">字符串匹配快捷方式</param>
    public StringShortcutAttribute(string shortcut)
    {
        StringShortcut = shortcut;
        Target = string.Empty;
    }
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
    /// 目标指令文本
    /// </summary>
    public string Target { get; }

    /// <summary>
    /// 构造特性实例
    /// </summary>
    /// <param name="pattern">正则式快捷方式</param>
    /// <param name="target">目标指令文本</param>
    public RegexShortcutAttribute(string pattern, string target)
    {
        RegexShortcut = new Regex(pattern);
        Target = target;
    }

    /// <summary>
    /// 构造特性实例
    /// </summary>
    /// <param name="pattern">正则式快捷方式</param>
    public RegexShortcutAttribute(string pattern)
    {
        RegexShortcut = new Regex(pattern);
        Target = string.Empty;
    }
}
