using Flandre.Core.Messaging;
using Flandre.Core.Models;

namespace Flandre.Adapters.Mock;

public class MockClient
{
    private readonly MockAdapter _adapter;

    public string GuildId { get; internal init; } = string.Empty;
    public string ChannelId { get; internal init; } = string.Empty;
    public string UserId { get; internal init; } = string.Empty;

    public MessageSourceType EnvironmentType { get; internal init; }

    internal MockClient(MockAdapter adapter)
    {
        _adapter = adapter;
    }

    private Message ConstructMessage(string message)
    {
        return new Message
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
    }

    public void SendMessage(string message)
    {
        var msg = ConstructMessage(message);
        _adapter.Bot.ReceiveMessage(msg);
    }

    public Task<MessageContent?> SendMessageForReply(string message) =>
        SendMessageForReply(message, TimeSpan.FromSeconds(10));

    public Task<MessageContent?> SendMessageForReply(string message, TimeSpan timeout)
    {
        var tcs = new TaskCompletionSource<MessageContent?>();

        var msg = ConstructMessage(message);
        _adapter.Bot.ReceiveMessageToReply(msg, tcs, timeout);

        return tcs.Task;
    }
}