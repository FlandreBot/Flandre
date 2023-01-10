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

    private readonly Dictionary<string, TaskCompletionSource<MessageContent?>> _tcsDict = new();

    internal void ReceiveMessage(Message message)
    {
        OnMessageReceived?.Invoke(this, new BotMessageReceivedEvent(message));
    }

    internal void ReceiveMessageToReply(Message message, TaskCompletionSource<MessageContent?> tcs, TimeSpan timeout)
    {
        _tcsDict[message.MessageId] = tcs;
        OnMessageReceived?.Invoke(this, new BotMessageReceivedEvent(message));
        Task.Run(async () =>
        {
            await Task.Delay(timeout);
            _tcsDict.GetValueOrDefault(message.MessageId)?.TrySetResult(null);
        });
    }

    public override Task<string?> SendMessage(MessageSourceType sourceType, string? channelId, string? userId,
        MessageContent content, string? guildId = null)
    {
        throw new NotSupportedException();
    }

    public override async Task<string?> SendMessage(Message message, MessageContent? contentOverride = null)
    {
        if (_tcsDict.TryGetValue(message.MessageId, out var tcs))
        {
            tcs.SetResult(contentOverride ?? message.Content);
            _tcsDict.Remove(message.MessageId);
        }

        return message.MessageId;
    }

    public override async Task<string?> SendChannelMessage(string channelId, MessageContent content,
        string? guildId = null)
    {
        await SendMessage(new Message
        {
            Platform = Platform,
            Content = content
        });
        return null;
    }

    public override async Task<string?> SendPrivateMessage(string userId, MessageContent content)
    {
        await SendMessage(new Message
        {
            Platform = Platform,
            Content = content
        });
        return null;
    }

    public override async Task<User?> GetSelf()
    {
        return new User
        {
            Name = "Test Bot",
            Nickname = "Test Bot",
            UserId = _selfId
        };
    }

    public override event BotEventHandler<BotMessageReceivedEvent>? OnMessageReceived;
    public override event BotEventHandler<BotGuildInvitedEvent>? OnGuildInvited;
    public override event BotEventHandler<BotGuildJoinRequestedEvent>? OnGuildJoinRequested;
    public override event BotEventHandler<BotFriendRequestedEvent>? OnFriendRequested;
}