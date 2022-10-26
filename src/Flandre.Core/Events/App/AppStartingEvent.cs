namespace Flandre.Core.Events.App;

/// <summary>
/// 应用正在启动事件
/// </summary>
public class AppStartingEvent : CancellableEvent
{
    internal AppStartingEvent()
    {
    }
}