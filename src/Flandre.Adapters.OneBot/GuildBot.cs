using Flandre.Adapters.OneBot.Models;
using Flandre.Core.Common;
using Flandre.Core.Events;
using Flandre.Core.Messaging;
using Flandre.Core.Models;

#pragma warning disable CS0067

namespace Flandre.Adapters.OneBot;

public class OneBotGuildBot : Bot
{
    /// <summary>
    /// Bot 平台名称，值为 qqguild
    /// </summary>
    public override string Platform => "qqguild";

    public override string SelfId => _selfId;

    private string _selfId = string.Empty;
    private bool _isSelfIdSet;

    public OneBotGuildInternalBot Internal { get; }

    public override event BotEventHandler<BotMessageReceivedEvent>? OnMessageReceived;
    public override event BotEventHandler<BotGuildInvitedEvent>? OnGuildInvited;
    public override event BotEventHandler<BotGuildJoinRequestedEvent>? OnGuildJoinRequested;
    public override event BotEventHandler<BotFriendRequestedEvent>? OnFriendRequested;

    internal OneBotGuildBot(OneBotBot mainBot)
    {
        Internal = new OneBotGuildInternalBot(mainBot);
    }

    internal void InvokeMessageEvent(OneBotApiGuildMessageEvent e)
    {
        if (!_isSelfIdSet)
        {
            _selfId = e.SelfId.ToString();
            _isSelfIdSet = true;
        }

        OnMessageReceived?.Invoke(this,
            new BotMessageReceivedEvent(new Message
            {
                Time = DateTimeOffset.FromUnixTimeSeconds(e.Time).DateTime,
                Platform = Platform,
                SourceType = MessageSourceType.Channel,
                MessageId = e.MessageId!,
                GuildId = e.GuildId,
                ChannelId = e.ChannelId,
                Sender = new User
                {
                    Name = e.Sender.Nickname,
                    UserId = e.Sender.TinyId!
                },
                Content = CqCodeParser.ParseCqMessage(e.Message!)
            }));
    }

    public override async Task<string?> SendMessageAsync(MessageSourceType sourceType, string? channelId, string? userId,
        MessageContent content, string? guildId = null)
    {
        if (sourceType == MessageSourceType.Channel)
            return await Internal.SendGuildChannelMessage(guildId!, channelId!, content);

        Log(BotLogLevel.Warning, "Platform qqguild does not support sending private message.");
        return null;
    }

    public override async Task<string?> SendChannelMessageAsync(string channelId, MessageContent content,
        string? guildId = null)
    {
        return await Internal.SendGuildChannelMessage(guildId!, channelId, content);
    }

    public override async Task<User?> GetSelfAsync()
    {
        var self = await Internal.GetGuildServiceProfile();
        return new User
        {
            Name = self.Nickname!,
            UserId = self.TinyId!,
            AvatarUrl = self.AvatarUrl
        };
    }

    public override Task<User?> GetUserAsync(string userId, string? guildId = null)
    {
        Log(BotLogLevel.Warning,
            $"Platform qqguild does not support method {nameof(GetUserAsync)}. If you need to get the information of guild member, please use method {nameof(GetGuildMemberAsync)} instead.");
        return Task.FromResult<User?>(null);
    }

    public override Task<IEnumerable<User>> GetFriendListAsync()
    {
        return Task.FromResult<IEnumerable<User>>(Array.Empty<User>());
    }

    public override async Task<Guild?> GetGuildAsync(string guildId)
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

    public override async Task<IEnumerable<Guild>> GetGuildListAsync()
    {
        return (await Internal.GetGuildList()).Select(g => new Guild
        {
            Id = g.GuildId!,
            Name = g.GuildName!
        });
    }

    public override async Task<GuildMember?> GetGuildMemberAsync(string guildId, string userId)
    {
        try
        {
            var user = await Internal.GetGuildMemberProfile(guildId, userId);
            return new GuildMember
            {
                Name = user.Nickname!,
                UserId = user.TinyId!,
                AvatarUrl = user.AvatarUrl,
                Roles = user.Roles?.Select(r => r.RoleName!).ToList() ?? new List<string>()
            };
        }
        catch
        {
            return null;
        }
    }

    public override async Task<IEnumerable<GuildMember>> GetGuildMemberListAsync(string guildId)
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
            UserId = m.TinyId!,
            Roles = new List<string> { m.RoleName! }
        });
    }

    public override async Task<Channel?> GetChannelAsync(string channelId, string? guildId = null)
    {
        return (await GetChannelListAsync(guildId!)).FirstOrDefault(c => c.Id == channelId);
    }

    public override async Task<IEnumerable<Channel>> GetChannelListAsync(string guildId)
    {
        return (await Internal.GetGuildChannelList(guildId)).Select(c => new Channel
        {
            Id = c.ChannelId!,
            Name = c.ChannelName!
        });
    }
}
