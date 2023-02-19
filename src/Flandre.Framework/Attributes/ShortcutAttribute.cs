using System.Text.RegularExpressions;

namespace Flandre.Framework.Attributes;

/// <summary>
/// 为指令添加前缀式快捷方式
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class StringShortcutAttribute : Attribute
{
    public string StringShortcut { get; }

    public StringShortcutAttribute(string shortcut)
    {
        StringShortcut = shortcut;
    }
}

/// <summary>
/// 为指令添加正则表达式式快捷方式
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RegexShortcutAttribute : Attribute
{
    public Regex RegexShortcut { get; }

    public RegexShortcutAttribute(string pattern)
    {
        RegexShortcut = new Regex(pattern);
    }
}