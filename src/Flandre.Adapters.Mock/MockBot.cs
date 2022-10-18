using Flandre.Core.Common;
using Flandre.Core.Events.Bot;
using Flandre.Core.Messaging;
using Flandre.Core.Models;
using Flandre.Core.Utils;

#pragma warning disable CS1998

namespace Flandre.Adapters.Mock;

public class MockBot : Bot
{
    /// <summary>
    /// Bot 平台名称，值为 test
    /// </summary>
    public override string Platform => "mock";

    private readonly Logger _logger;

    private readonly string _selfId = Guid.NewGuid().ToString();

    private TaskCompletionSource<MessageContent?>? _tcs;

    private MessageSourceType _sourceType = MessageSourceType.Channel;

    protected override Logger GetLogger() => _logger;

    internal MockBot(Logger logger)
    {
        _logger = logger;
    }

    internal void ReceiveMessage(Message message, TaskCompletionSource<MessageContent?> tcs)
    {
        _tcs = tcs;
        _sourceType = message.SourceType;
        OnMessageReceived?.Invoke(this, new BotMessageReceivedEvent(message));
    }

    public override Task Start() => Task.CompletedTask;

    public override Task Stop() => Task.CompletedTask;

    public override Task<string?> SendMessage(MessageSourceType sourceType, string? channelId, string? userId,
        MessageContent content, string? guildId = null)
    {
        return SendMessage(new Message { Content = content });
    }

    public override async Task<string?> SendMessage(Message message, MessageContent? contentOverride = null)
    {
        _tcs?.SetResult(contentOverride ?? message.Content);
        return null;
    }

    public override async Task<string?> SendChannelMessage(string channelId, MessageContent content,
        string? guildId = null)
    {
        if (_tcs is null) return null;
        if (_sourceType != MessageSourceType.Channel) return null;
        await SendMessage(new Message { Content = content });
        return null;
    }

    public override async Task<string?> SendPrivateMessage(string userId, MessageContent content)
    {
        if (_tcs is null) return null;
        if (_sourceType != MessageSourceType.Private) return null;
        await SendMessage(new Message { Content = content });
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