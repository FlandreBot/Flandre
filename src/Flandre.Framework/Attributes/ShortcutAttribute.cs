using System.Text.RegularExpressions;

namespace Flandre.Framework.Attributes;

/// <summary>
/// 为指令添加快捷方式
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ShortcutAttribute : Attribute
{
    public string? StringShortcut { get; set; }
    public Regex? RegexShortcut { get; set; }

    public ShortcutAttribute(string shortcut)
    {
        StringShortcut = shortcut;
    }

    public ShortcutAttribute(Regex shortcut)
    {
        RegexShortcut = shortcut;
    }
}