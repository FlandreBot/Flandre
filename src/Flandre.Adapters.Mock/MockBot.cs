using Flandre.Core.Common;
using Flandre.Core.Events;
using Flandre.Core.Messaging;
using Flandre.Core.Models;

#pragma warning disable CS1998

namespace Flandre.Adapters.Mock;

public class MockBot : Bot
{
    /// <summary>
    /// Bot 平台名称，值为 mock
    /// </summary>
    public override string Platform => "mock";

    public override string SelfId => _selfId;

    private readonly string _selfId = Guid.NewGuid().ToString();

    internal (string MessageId, TaskCompletionSource<MessageContent?> Tcs)? ReplyTarget { get; set; }

    internal void ReceiveMessage(Message message)
    {
        MessageReceived?.Invoke(this, new BotMessageReceivedEvent(message));
    }

    private void Send(MessageContent? content)
    {
        ReplyTarget?.Tcs.TrySetResult(content);
    }

    public override async Task<string?> SendChannelMessageAsync(string channelId, MessageContent content,
        string? guildId = null)
    {
        Send(content);
        return null;
    }

    public override async Task<string?> SendPrivateMessageAsync(string userId, MessageContent content)
    {
        Send(content);
        return null;
    }

    public override async Task<User?> GetSelfAsync()
    {
        return new User
        {
            Name = "Test Bot",
            Nickname = "Test Bot",
            UserId = _selfId
        };
    }

    public override event BotEventHandler<BotMessageReceivedEvent>? MessageReceived;
    public override event BotEventHandler<BotGuildInvitedEvent>? GuildInvited;
    public override event BotEventHandler<BotGuildJoinRequestedEvent>? GuildJoinRequested;
    public override event BotEventHandler<BotFriendRequestedEvent>? FriendRequested;
}
