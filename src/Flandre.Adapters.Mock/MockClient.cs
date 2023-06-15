using Flandre.Core.Messaging;
using Flandre.Core.Models;

namespace Flandre.Adapters.Mock;

public class MockClient
{
    private readonly MockAdapter _adapter;

    public string GuildId { get; internal init; } = string.Empty;
    public string ChannelId { get; internal init; } = string.Empty;
    public string UserId { get; internal init; } = string.Empty;

    public MessageEnvironment EnvironmentType { get; internal init; }

    internal MockClient(MockAdapter adapter)
    {
        _adapter = adapter;
    }

    private Message ConstructMessage(string message)
    {
        return new Message
        {
            Time = DateTime.Now,
            Environment = EnvironmentType,
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

    public Task<MessageContent?> SendMessageForReplyAsync(string message)
    {
        return SendMessageForReplyAsync(message, TimeSpan.FromSeconds(10));
    }

    public Task<MessageContent?> SendMessageForReplyAsync(string message, TimeSpan timeout)
    {
        var tcs = new TaskCompletionSource<MessageContent?>();

        var msg = ConstructMessage(message);

        _adapter.Bot.ReplyTarget = (msg.MessageId, tcs);
        _adapter.Bot.ReceiveMessage(msg);

        Task.Run(async () =>
        {
            await Task.Delay(timeout);
            if (_adapter.Bot.ReplyTarget is { } target
                && target.MessageId == msg.MessageId
                && !target.Tcs.Task.IsCompleted)
            {
                _adapter.Bot.ReplyTarget = null;
                tcs.TrySetResult(null);
            }
        });

        return tcs.Task;
    }
}
