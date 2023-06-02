using System.Reactive.Linq;
using Flandre.Core.Common;
using Flandre.Core.Events;

namespace Flandre.Core.Reactive;

public static class CoreReactiveExtensions
{
    private static IObservable<TEvent> OnBotEvent<TEvent>(
        Action<BotEventHandler<TEvent>> add, Action<BotEventHandler<TEvent>> remove) where TEvent : FlandreEvent
    {
        return Observable.FromEventPattern<BotEventHandler<TEvent>, TEvent>(add, remove)
            .Select(pattern => pattern.EventArgs);
    }

    public static IObservable<BotLoggingEvent> OnLogging(this Bot bot)
    {
        return OnBotEvent<BotLoggingEvent>(
            add => bot.Logging += add,
            remove => bot.Logging -= remove);
    }

    public static IObservable<BotMessageReceivedEvent> OnMessageReceived(this Bot bot)
    {
        return OnBotEvent<BotMessageReceivedEvent>(
            add => bot.MessageReceived += add,
            remove => bot.MessageReceived -= remove);
    }

    public static IObservable<BotGuildInvitedEvent> OnGuildInvited(this Bot bot)
    {
        return OnBotEvent<BotGuildInvitedEvent>(
            add => bot.GuildInvited += add,
            remove => bot.GuildInvited -= remove);
    }

    public static IObservable<BotGuildJoinRequestedEvent> OnGuildJoinRequested(this Bot bot)
    {
        return OnBotEvent<BotGuildJoinRequestedEvent>(
            add => bot.GuildJoinRequested += add,
            remove => bot.GuildJoinRequested -= remove);
    }

    public static IObservable<BotFriendRequestedEvent> OnFriendRequested(this Bot bot)
    {
        return OnBotEvent<BotFriendRequestedEvent>(
            add => bot.FriendRequested += add,
            remove => bot.FriendRequested -= remove);
    }
}
