using System.Reactive.Linq;
using Flandre.Core.Events;
using Flandre.Core.Messaging;

namespace Flandre.Core.Reactive;

public static class ObservableMessageReceivedExtensions
{
    public static IObservable<BotMessageReceivedEvent> OfPlatform(
        this IObservable<BotMessageReceivedEvent> observable,
        params string[] platforms)
    {
        return observable.Where(e =>
            platforms.Any(p => e.Message.Platform.Equals(p, StringComparison.OrdinalIgnoreCase)));
    }

    public static IObservable<BotMessageReceivedEvent> OfUser(
        this IObservable<BotMessageReceivedEvent> observable,
        params string[] userIds)
    {
        return observable.Where(e => userIds.Contains(e.Message.Sender.UserId));
    }

    public static IObservable<BotMessageReceivedEvent> OfGuild(
        this IObservable<BotMessageReceivedEvent> observable,
        params string[] guildIds)
    {
        return observable.Where(e => guildIds.Contains(e.Message.GuildId));
    }

    public static IObservable<BotMessageReceivedEvent> OfChannel(
        this IObservable<BotMessageReceivedEvent> observable,
        params string[] channelIds)
    {
        return observable.Where(e => channelIds.Contains(e.Message.ChannelId));
    }

    public static IObservable<BotMessageReceivedEvent> InPrivate(this IObservable<BotMessageReceivedEvent> observable)
    {
        return observable.Where(e => e.Message.Environment == MessageEnvironment.Private);
    }

    public static IObservable<BotMessageReceivedEvent> InChannel(this IObservable<BotMessageReceivedEvent> observable)
    {
        return observable.Where(e => e.Message.Environment == MessageEnvironment.Channel);
    }
}
