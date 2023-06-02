using System.Reactive.Linq;
using Flandre.Core.Events;
using Flandre.Framework.Events;

namespace Flandre.Framework.Reactive;

public static class AppReactiveExtensions
{
    private static IObservable<TEvent> ObserveAppEvent<TEvent>(
        Action<AppEventHandler<TEvent>> add, Action<AppEventHandler<TEvent>> remove) where TEvent : FlandreEvent
    {
        return Observable.FromEventPattern<AppEventHandler<TEvent>, TEvent>(add, remove)
            .Select(pattern => pattern.EventArgs);
    }

    public static IObservable<AppStartingEvent> ObserveStarting(this FlandreApp app)
    {
        return ObserveAppEvent<AppStartingEvent>(
            add => app.Starting += add,
            remove => app.Starting -= remove);
    }

    public static IObservable<AppReadyEvent> ObserveReady(this FlandreApp app)
    {
        return ObserveAppEvent<AppReadyEvent>(
            add => app.Ready += add,
            remove => app.Ready -= remove);
    }

    public static IObservable<AppStoppedEvent> ObserveStopped(this FlandreApp app)
    {
        return ObserveAppEvent<AppStoppedEvent>(
            add => app.Stopped += add,
            remove => app.Stopped -= remove);
    }

    public static IObservable<CommandInvokingEvent> ObserveCommandInvoking(this FlandreApp app)
    {
        return ObserveAppEvent<CommandInvokingEvent>(
            add => app.CommandInvoking += add,
            remove => app.CommandInvoking -= remove);
    }

    public static IObservable<CommandInvokedEvent> ObserveCommandInvoked(this FlandreApp app)
    {
        return ObserveAppEvent<CommandInvokedEvent>(
            add => app.CommandInvoked += add,
            remove => app.CommandInvoked -= remove);
    }
}
