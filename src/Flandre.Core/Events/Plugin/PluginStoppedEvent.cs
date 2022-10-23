namespace Flandre.Core.Events.Plugin;

/// <summary>
/// 插件关闭事件
/// </summary>
public class PluginStoppedEvent : BaseEvent
{
    /// <summary>
    /// 将要关闭的插件
    /// </summary>
    public Common.Plugin Plugin { get; }

    internal PluginStoppedEvent(Common.Plugin plugin)
    {
        Plugin = plugin;
    }
}