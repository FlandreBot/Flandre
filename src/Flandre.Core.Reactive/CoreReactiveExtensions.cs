using System.Reactive.Linq;
using Flandre.Core.Common;
using Flandre.Core.Events;

namespace Flandre.Core.Reactive;

public static class CoreReactiveExtensions
{
    private static IObservable<TEvent> ObserveBotEvent<TEvent>(
        Action<BotEventHandler<TEvent>> add, Action<BotEventHandler<TEvent>> remove) where TEvent : FlandreEvent
    {
        return Observable.FromEventPattern<BotEventHandler<TEvent>, TEvent>(add, remove)
            .Select(pattern => pattern.EventArgs);
    }

    public static IObservable<BotLoggingEvent> ObserveLogging(this Bot bot)
    {
        return ObserveBotEvent<BotLoggingEvent>(
            add => bot.Logging += add,
            remove => bot.Logging -= remove);
    }

    public static IObservable<BotMessageReceivedEvent> ObserveMessageReceived(this Bot bot)
    {
        return ObserveBotEvent<BotMessageReceivedEvent>(
            add => bot.MessageReceived += add,
            remove => bot.MessageReceived -= remove);
    }

    public static IObservable<BotGuildInvitedEvent> ObserveGuildInvited(this Bot bot)
    {
        return ObserveBotEvent<BotGuildInvitedEvent>(
            add => bot.GuildInvited += add,
            remove => bot.GuildInvited -= remove);
    }

    public static IObservable<BotGuildJoinRequestedEvent> ObserveGuildJoinRequested(this Bot bot)
    {
        return ObserveBotEvent<BotGuildJoinRequestedEvent>(
            add => bot.GuildJoinRequested += add,
            remove => bot.GuildJoinRequested -= remove);
    }

    public static IObservable<BotFriendRequestedEvent> ObserveFriendRequested(this Bot bot)
    {
        return ObserveBotEvent<BotFriendRequestedEvent>(
            add => bot.FriendRequested += add,
            remove => bot.FriendRequested -= remove);
    }
}
