using System.Reactive.Linq;
using Flandre.Core.Common;
using Flandre.Core.Events;

namespace Flandre.Core.Reactive;

public static class CoreReactiveExtensions
{
    private static IObservable<TEvent> ObserveBotEvent<TEvent>(
        Action<BotEventHandler<TEvent>> add, Action<BotEventHandler<TEvent>> remove) where TEvent : BaseEvent
    {
        return Observable.FromEventPattern<BotEventHandler<TEvent>, TEvent>(add, remove)
            .Select(pattern => pattern.EventArgs);
    }

    public static IObservable<BotLoggingEvent> ObserveLogging(this Bot bot)
    {
        return ObserveBotEvent<BotLoggingEvent>(
            add => bot.OnLogging += add,
            remove => bot.OnLogging -= remove);
    }

    public static IObservable<BotMessageReceivedEvent> ObserveMessageReceived(this Bot bot)
    {
        return ObserveBotEvent<BotMessageReceivedEvent>(
            add => bot.OnMessageReceived += add,
            remove => bot.OnMessageReceived -= remove);
    }

    public static IObservable<BotGuildInvitedEvent> ObserveGuildInvited(this Bot bot)
    {
        return ObserveBotEvent<BotGuildInvitedEvent>(
            add => bot.OnGuildInvited += add,
            remove => bot.OnGuildInvited -= remove);
    }

    public static IObservable<BotGuildJoinRequestedEvent> ObserveGuildJoinRequested(this Bot bot)
    {
        return ObserveBotEvent<BotGuildJoinRequestedEvent>(
            add => bot.OnGuildJoinRequested += add,
            remove => bot.OnGuildJoinRequested -= remove);
    }

    public static IObservable<BotFriendRequestedEvent> ObserveFriendRequested(this Bot bot)
    {
        return ObserveBotEvent<BotFriendRequestedEvent>(
            add => bot.OnFriendRequested += add,
            remove => bot.OnFriendRequested -= remove);
    }
}