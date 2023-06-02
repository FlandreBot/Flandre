using System.Reactive.Linq;
using Flandre.Core.Events;
using Flandre.Framework.Events;

namespace Flandre.Framework.Reactive;

public static class AppReactiveExtensions
{
    private static IObservable<TEvent> OnAppEvent<TEvent>(
        Action<AppEventHandler<TEvent>> add, Action<AppEventHandler<TEvent>> remove) where TEvent : FlandreEvent
    {
        return Observable.FromEventPattern<AppEventHandler<TEvent>, TEvent>(add, remove)
            .Select(pattern => pattern.EventArgs);
    }

    public static IObservable<AppStartingEvent> OnStarting(this FlandreApp app)
    {
        return OnAppEvent<AppStartingEvent>(
            add => app.Starting += add,
            remove => app.Starting -= remove);
    }

    public static IObservable<AppReadyEvent> OnReady(this FlandreApp app)
    {
        return OnAppEvent<AppReadyEvent>(
            add => app.Ready += add,
            remove => app.Ready -= remove);
    }

    public static IObservable<AppStoppedEvent> OnStopped(this FlandreApp app)
    {
        return OnAppEvent<AppStoppedEvent>(
            add => app.Stopped += add,
            remove => app.Stopped -= remove);
    }

    public static IObservable<CommandInvokingEvent> OnCommandInvoking(this FlandreApp app)
    {
        return OnAppEvent<CommandInvokingEvent>(
            add => app.CommandInvoking += add,
            remove => app.CommandInvoking -= remove);
    }

    public static IObservable<CommandInvokedEvent> OnCommandInvoked(this FlandreApp app)
    {
        return OnAppEvent<CommandInvokedEvent>(
            add => app.CommandInvoked += add,
            remove => app.CommandInvoked -= remove);
    }
}
