namespace Flandre.Core.Events.Plugin;

/// <summary>
/// 插件启动事件
/// </summary>
public class PluginStartingEvent : BaseEvent, ICancellableEvent
{
    /// <summary>
    /// 将要启动的插件
    /// </summary>
    public Common.Plugin Plugin { get; }

    internal bool Cancelled;

    internal PluginStartingEvent(Common.Plugin plugin)
    {
        Plugin = plugin;
    }

    /// <summary>
    /// 取消启动插件
    /// </summary>
    public void Cancel() => Cancelled = true;
}