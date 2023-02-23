using System.Reactive.Linq;
using Flandre.Core.Events;
using Flandre.Framework.Events;

namespace Flandre.Framework.Reactive;

public static class AppReactiveExtensions
{
    private static IObservable<TEvent> ObserveAppEvent<TEvent>(
        Action<AppEventHandler<TEvent>> add, Action<AppEventHandler<TEvent>> remove) where TEvent : BaseEvent
    {
        return Observable.FromEventPattern<AppEventHandler<TEvent>, TEvent>(add, remove)
            .Select(pattern => pattern.EventArgs);
    }

    public static IObservable<AppStartingEvent> ObserveStarting(this FlandreApp app)
    {
        return ObserveAppEvent<AppStartingEvent>(
            add => app.OnStarting += add,
            remove => app.OnStarting -= remove);
    }

    public static IObservable<AppReadyEvent> ObserveReady(this FlandreApp app)
    {
        return ObserveAppEvent<AppReadyEvent>(
            add => app.OnReady += add,
            remove => app.OnReady -= remove);
    }

    public static IObservable<AppStoppedEvent> ObserveStopped(this FlandreApp app)
    {
        return ObserveAppEvent<AppStoppedEvent>(
            add => app.OnStopped += add,
            remove => app.OnStopped -= remove);
    }

    public static IObservable<CommandInvokingEvent> ObserveCommandInvoking(this FlandreApp app)
    {
        return ObserveAppEvent<CommandInvokingEvent>(
            add => app.OnCommandInvoking += add,
            remove => app.OnCommandInvoking -= remove);
    }

    public static IObservable<CommandInvokedEvent> ObserveCommandInvoked(this FlandreApp app)
    {
        return ObserveAppEvent<CommandInvokedEvent>(
            add => app.OnCommandInvoked += add,
            remove => app.OnCommandInvoked -= remove);
    }
}
