namespace Flandre.Core.Events;

public class BaseEvent : EventArgs
{
    public DateTime EventTime { get; }

    internal BaseEvent()
    {
        EventTime = DateTime.Now;
    }
}