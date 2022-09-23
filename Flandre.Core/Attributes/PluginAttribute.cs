namespace Flandre.Core.Attributes;

/// <summary>
/// 插件定义
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PluginAttribute : Attribute
{
    /// <summary>
    /// 插件名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 插件根指令
    /// </summary>
    public string? BaseCommand { get; set; }

    /// <summary>
    /// 定义插件
    /// </summary>
    /// <param name="name">插件名称</param>
    public PluginAttribute(string name)
    {
        Name = name;
    }
}