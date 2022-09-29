using Flandre.Core.Messaging;

namespace Flandre.TestKit;

public static class FlandreTestingExtensions
{
    public static FlandreTestClient GenerateChannelClient(this TestAdapter adapter, string guildId, string channelId,
        string userId)
    {
        return new FlandreTestClient(adapter)
        {
            EnvironmentType = MessageSourceType.Channel,
            GuildId = guildId,
            ChannelId = channelId,
            UserId = userId
        };
    }

    public static FlandreTestClient GenerateChannelClient(this TestAdapter adapter)
        => GenerateChannelClient(adapter, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString());

    public static FlandreTestClient GenerateFriendClient(this TestAdapter adapter, string userId)
    {
        return new FlandreTestClient(adapter)
        {
            EnvironmentType = MessageSourceType.Private,
            UserId = userId
        };
    }

    public static FlandreTestClient GenerateFriendClient(this TestAdapter adapter)
        => GenerateFriendClient(adapter, Guid.NewGuid().ToString());
}