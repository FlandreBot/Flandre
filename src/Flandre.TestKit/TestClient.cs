using Flandre.Core.Messaging;
using Flandre.Core.Models;

namespace Flandre.TestKit;

public class FlandreTestClient
{
    private readonly TestAdapter _adapter;

    public string GuildId { get; internal init; } = "";
    public string ChannelId { get; internal init; } = "";
    public string UserId { get; internal init; } = "";

    public MessageSourceType EnvironmentType { get; internal init; }

    internal MessageContent? CurrentMessage;

    internal CancellationTokenSource? Cancellation;

    internal FlandreTestClient(TestAdapter adapter)
    {
        _adapter = adapter;
    }

    public async Task<MessageContent?> SendForReply(string message, int waitSeconds = 10)
    {
        Cancellation = new CancellationTokenSource();
        CurrentMessage = null;

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
                Id = UserId,
                AvatarUrl = null,
                Roles = new List<string>()
            },
            Content = message
        };
        _adapter.Bot.ReceiveMessage(msg, this);

        try
        {
            await Task.Delay(waitSeconds * 1000, Cancellation.Token);
        }
        catch
        {
            // ignored
        }

        return CurrentMessage;
    }
}