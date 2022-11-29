using Flandre.Core.Events;

namespace Flandre.Framework.Events;

/// <summary>
/// 应用正在启动事件
/// </summary>
public sealed class AppStartingEvent : BaseEvent
{
    internal AppStartingEvent()
    {
    }
}