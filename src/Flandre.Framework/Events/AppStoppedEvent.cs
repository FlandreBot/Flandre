using Flandre.Core.Events;

namespace Flandre.Framework.Events;

/// <summary>
/// 应用退出事件
/// </summary>
public sealed class AppStoppedEvent : BaseEvent
{
    internal AppStoppedEvent()
    {
    }
}