using System.Reactive.Linq;
using Flandre.Core.Messaging;

namespace Flandre.Core.Reactive;

public static class ObservableMessageExtensions
{
    public static IObservable<Message> OfPlatform(
        this IObservable<Message> observable,
        params string[] platforms)
    {
        return observable.Where(msg =>
            platforms.Any(p => msg.Platform.Equals(p, StringComparison.OrdinalIgnoreCase)));
    }

    public static IObservable<Message> OfUser(
        this IObservable<Message> observable,
        params string[] userIds)
    {
        return observable.Where(msg => userIds.Contains(msg.Sender.UserId));
    }

    public static IObservable<Message> OfGuild(
        this IObservable<Message> observable,
        params string[] guildIds)
    {
        return observable.Where(msg => guildIds.Contains(msg.GuildId));
    }

    public static IObservable<Message> OfChannel(
        this IObservable<Message> observable,
        params string[] channelIds)
    {
        return observable.Where(msg => channelIds.Contains(msg.ChannelId));
    }

    public static IObservable<Message> InPrivate(this IObservable<Message> observable)
    {
        return observable.Where(msg => msg.Environment == MessageEnvironment.Private);
    }

    public static IObservable<Message> InChannel(this IObservable<Message> observable)
    {
        return observable.Where(msg => msg.Environment == MessageEnvironment.Channel);
    }
}
