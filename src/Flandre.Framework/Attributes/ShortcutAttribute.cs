namespace Flandre.Framework.Attributes;

/// <summary>
/// 为指令添加快捷方式
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ShortcutAttribute : Attribute
{
    /// <summary>
    /// 指令快捷方式
    /// </summary>
    public string Shortcut { get; set; }

    /// <summary>
    /// 快捷方式特性构造函数
    /// </summary>
    /// <param name="shortcut">快捷方式名称</param>
    public ShortcutAttribute(string shortcut)
    {
        Shortcut = shortcut;
    }
}