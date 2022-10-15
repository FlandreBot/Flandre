using Flandre.Adapters.OneBot.Models;
using Flandre.Core.Common;
using Flandre.Core.Events.Bot;
using Flandre.Core.Messaging;
using Flandre.Core.Models;

namespace Flandre.Adapters.OneBot;

public class OneBotGuildBot : IBot
{
    public OneBotGuildInternalBot Internal { get; }

    private readonly OneBotBot _mainBot;

    internal OneBotGuildBot(OneBotBot mainBot)
    {
        Internal = new OneBotGuildInternalBot(mainBot);
        _mainBot = mainBot;
    }

    internal void InvokeMessageEvent(OneBotApiGuildMessageEvent e)
    {
        OnMessageReceived?.Invoke(this, new BotMessageReceivedEvent(new Message
        {
            Time = DateTimeOffset.FromUnixTimeSeconds(e.Time).DateTime,
            SourceType = MessageSourceType.Channel,
            MessageId = e.MessageId!,
            GuildId = e.GuildId,
            ChannelId = e.ChannelId,
            Sender = new User
            {
                Name = e.Sender.Nickname,
                Id = e.Sender.TinyId!
            },
            Content = CqCodeParser.ParseCqMessage(e.RawMessage)
        }));
    }

    public Task Start()
    {
        return Task.CompletedTask;
    }

    public Task Stop()
    {
        return Task.CompletedTask;
    }

    public async Task<string?> SendMessage(MessageSourceType sourceType, string? channelId, string? userId,
        MessageContent content, string? guildId = null)
    {
        if (sourceType == MessageSourceType.Channel)
            return await Internal.SendGuildChannelMessage(guildId!, channelId!, content);

        _mainBot.Logger.Warning("OneBot 暂不支持发送频道私聊消息。");
        return null;
    }

    public Task<string?> SendMessage(Message message, MessageContent? contentOverride = null)
    {
        return SendMessage(message.SourceType, message.ChannelId, message.Sender.Id,
            contentOverride ?? message.Content, message.GuildId);
    }

    public async Task<string?> SendChannelMessage(string channelId, MessageContent content, string? guildId = null)
    {
        return await Internal.SendGuildChannelMessage(guildId!, channelId, content);
    }

    public Task<string?> SendPrivateMessage(string userId, MessageContent content)
    {
        _mainBot.Logger.Warning("OneBot 暂不支持发送频道私聊消息。");
        return Task.FromResult<string?>(null);
    }

    public Task DeleteMessage(string messageId)
    {
        _mainBot.Logger.Warning("OneBot 暂不支持撤回频道消息。");
        return Task.FromResult<string?>(null);
    }

    public async Task<User> GetSelf()
    {
        var self = await Internal.GetGuildServiceProfile();
        return new User
        {
            Name = self.Nickname!,
            Id = self.TinyId!,
            AvatarUrl = self.AvatarUrl
        };
    }

    public Task<User?> GetUser(string userId, string? guildId = null)
    {
        _mainBot.Logger.Warning("OneBot 暂不支持获取频道用户信息。若您需要获取频道成员信息，请使用 GetGuildMember 方法代替。");
        return Task.FromResult<User?>(null);
    }

    public Task<IEnumerable<User>> GetFriendList()
    {
        return Task.FromResult<IEnumerable<User>>(Array.Empty<User>());
    }

    public async Task<Guild?> GetGuild(string guildId)
    {
        try
        {
            var guild = await Internal.GetGuildMetaByGuest(guildId);
            return new Guild
            {
                Id = guild.GuildId!,
                Name = guild.GuildName!
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<IEnumerable<Guild>> GetGuildList()
    {
        return (await Internal.GetGuildList()).Select(g => new Guild
        {
            Id = g.GuildId!,
            Name = g.GuildName!
        });
    }

    public async Task<GuildMember?> GetGuildMember(string guildId, string userId)
    {
        try
        {
            var user = await Internal.GetGuildMemberProfile(guildId, userId);
            return new GuildMember
            {
                Name = user.Nickname!,
                Id = user.TinyId!,
                AvatarUrl = user.AvatarUrl,
                Roles = user.Roles?.Select(r => r.RoleName!).ToList() ?? new List<string>()
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<IEnumerable<GuildMember>> GetGuildMemberList(string guildId)
    {
        var list = new List<OneBotGuildMember>();
        OneBotGuildMemberListResponse resp;
        var nextToken = "";

        do
        {
            resp = await Internal.GetGuildMemberList(guildId, nextToken);
            nextToken = resp.NextToken!;
            list.AddRange(resp.Members!);
        } while (!resp.Finished);

        return list.Select(m => new GuildMember
        {
            Name = m.Nickname!,
            Id = m.TinyId!,
            Roles = new List<string> { m.RoleName! }
        });
    }

    public async Task<Channel?> GetChannel(string channelId, string? guildId = null)
    {
        return (await GetChannelList(guildId!)).FirstOrDefault(c => c.Id == channelId);
    }

    public async Task<IEnumerable<Channel>> GetChannelList(string guildId)
    {
        return (await Internal.GetGuildChannelList(guildId)).Select(c => new Channel
        {
            Id = c.ChannelId!,
            Name = c.ChannelName!
        });
    }

    public event IBot.BotEventHandler<BotMessageReceivedEvent>? OnMessageReceived;
    public event IBot.BotEventHandler<BotGuildInvitedEvent>? OnGuildInvited;
    public event IBot.BotEventHandler<BotGuildRequestedEvent>? OnGuildRequested;
    public event IBot.BotEventHandler<BotFriendRequestedEvent>? OnFriendRequested;

    public Task HandleGuildInvitation(BotGuildInvitedEvent e, bool approve, string? comment = null)
    {
        _mainBot.Logger.Warning("OneBot 暂不支持处理频道邀请。");
        return Task.CompletedTask;
    }

    public Task HandleGuildRequest(BotGuildRequestedEvent e, bool approve, string? comment = null)
    {
        _mainBot.Logger.Warning("OneBot 暂不支持处理频道申请。");
        return Task.CompletedTask;
    }

    public Task HandleFriendRequest(BotFriendRequestedEvent e, bool approve, string? comment = null)
    {
        _mainBot.Logger.Warning("OneBot 暂不支持处理频道好友申请。");
        return Task.CompletedTask;
    }
}