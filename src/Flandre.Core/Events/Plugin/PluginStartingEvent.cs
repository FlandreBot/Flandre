namespace Flandre.Core.Events.Plugin;

/// <summary>
/// 插件启动事件
/// </summary>
public class PluginStartingEvent : CancellableEvent
{
    /// <summary>
    /// 将要启动的插件
    /// </summary>
    public Common.Plugin Plugin { get; }

    internal PluginStartingEvent(Common.Plugin plugin)
    {
        Plugin = plugin;
    }
}