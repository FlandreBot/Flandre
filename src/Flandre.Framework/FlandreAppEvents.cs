using Flandre.Core.Events;
using Flandre.Framework.Events;

namespace Flandre.Framework;

public sealed partial class FlandreApp
{
    public delegate void AppEventHandler<in TEvent>(FlandreApp app, TEvent e) where TEvent : BaseEvent;

    public event AppEventHandler<AppStartingEvent>? OnStarting;

    public event AppEventHandler<AppReadyEvent>? OnReady;

    public event AppEventHandler<AppStoppedEvent>? OnStopped;

    public event AppEventHandler<CommandInvokingEvent>? OnCommandInvoking;

    public event AppEventHandler<CommandInvokedEvent>? OnCommandInvoked;
}