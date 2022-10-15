using Flandre.Core.Common;
using Flandre.Core.Events.Bot;
using Flandre.Core.Messaging;
using Flandre.Core.Models;

#pragma warning disable CS1998

namespace Flandre.TestKit;

public class TestBot : IBot
{
    private readonly string _selfId = Guid.NewGuid().ToString();

    private TaskCompletionSource<MessageContent?>? _tcs;

    private MessageSourceType _sourceType = MessageSourceType.Channel;

    internal void ReceiveMessage(Message message, TaskCompletionSource<MessageContent?> tcs)
    {
        _tcs = tcs;
        _sourceType = message.SourceType;
        OnMessageReceived?.Invoke(this, new BotMessageReceivedEvent(message));
    }

    public async Task Start()
    {
    }

    public async Task Stop()
    {
    }

    public Task<string?> SendMessage(MessageSourceType sourceType, string? channelId, string? userId,
        MessageContent content, string? guildId = null)
    {
        return SendMessage(new Message { Content = content });
    }

    public async Task<string?> SendMessage(Message message, MessageContent? contentOverride = null)
    {
        _tcs?.SetResult(contentOverride ?? message.Content);
        return null;
    }

    public async Task<string?> SendChannelMessage(string channelId, MessageContent content, string? guildId = null)
    {
        if (_tcs is null) return null;
        if (_sourceType != MessageSourceType.Channel) return null;
        await SendMessage(new Message { Content = content });
        return null;
    }

    public async Task<string?> SendPrivateMessage(string userId, MessageContent content)
    {
        if (_tcs is null) return null;
        if (_sourceType != MessageSourceType.Private) return null;
        await SendMessage(new Message { Content = content });
        return null;
    }

    public async Task DeleteMessage(string messageId)
    {
    }

    public async Task<User> GetSelf()
    {
        return new User
        {
            Name = "Test Bot",
            Nickname = "Test Bot",
            Id = _selfId
        };
    }

    public async Task<User?> GetUser(string userId, string? guildId = null)
    {
        return null;
    }

    public async Task<IEnumerable<User>> GetFriendList()
    {
        return new List<User>();
    }

    public async Task<Guild?> GetGuild(string guildId)
    {
        return null;
    }

    public async Task<IEnumerable<Guild>> GetGuildList()
    {
        return new List<Guild>();
    }

    public async Task<GuildMember?> GetGuildMember(string guildId, string userId)
    {
        return null;
    }

    public async Task<IEnumerable<GuildMember>> GetGuildMemberList(string guildId)
    {
        return new List<GuildMember>();
    }

    public async Task<Channel?> GetChannel(string channelId, string? guildId = null)
    {
        return null;
    }

    public async Task<IEnumerable<Channel>> GetChannelList(string guildId)
    {
        return new List<Channel>();
    }

    public event IBot.BotEventHandler<BotMessageReceivedEvent>? OnMessageReceived;
    public event IBot.BotEventHandler<BotGuildInvitedEvent>? OnGuildInvited;
    public event IBot.BotEventHandler<BotGuildRequestedEvent>? OnGuildRequested;
    public event IBot.BotEventHandler<BotFriendRequestedEvent>? OnFriendRequested;

    public async Task HandleGuildInvitation(BotGuildInvitedEvent e, bool approve, string? comment = null)
    {
    }

    public async Task HandleGuildRequest(BotGuildRequestedEvent e, bool approve, string? comment = null)
    {
    }

    public async Task HandleFriendRequest(BotFriendRequestedEvent e, bool approve, string? comment = null)
    {
    }
}