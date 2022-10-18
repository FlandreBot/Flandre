using Flandre.Core.Messaging;
using Flandre.Core.Models;

namespace Flandre.Adapters.Mock;

public class MockClient
{
    private readonly MockAdapter _adapter;

    public string GuildId { get; internal init; } = "";
    public string ChannelId { get; internal init; } = "";
    public string UserId { get; internal init; } = "";

    public MessageSourceType EnvironmentType { get; internal init; }

    internal MockClient(MockAdapter adapter)
    {
        _adapter = adapter;
    }

    public Task<MessageContent?> SendForReply(string message)
    {
        var tcs = new TaskCompletionSource<MessageContent?>();

        var msg = new Message
        {
            Time = DateTime.Now,
            SourceType = EnvironmentType,
            MessageId = Guid.NewGuid().ToString(),
            GuildId = GuildId,
            ChannelId = ChannelId,
            Sender = new GuildMember
            {
                Name = "Test Client",
                Nickname = "Test Client",
                UserId = UserId,
                AvatarUrl = null,
                Roles = new List<string>()
            },
            Content = message
        };
        _adapter.Bot.ReceiveMessage(msg, tcs);

        return tcs.Task;
    }
}