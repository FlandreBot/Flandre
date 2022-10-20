using Flandre.Adapters.OneBot.Models;
using Flandre.Core.Common;
using Flandre.Core.Events.Bot;
using Flandre.Core.Messaging;
using Flandre.Core.Models;
using Flandre.Core.Utils;

namespace Flandre.Adapters.OneBot;

public class OneBotGuildBot : Bot
{
    /// <summary>
    /// Bot 平台名称，值为 qqguild
    /// </summary>
    public override string Platform => "qqguild";

    public OneBotGuildInternalBot Internal { get; }

    private readonly OneBotBot _mainBot;

    protected override Logger GetLogger() => _mainBot.Logger;

    public override event BotEventHandler<BotMessageReceivedEvent>? OnMessageReceived;

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
                UserId = e.Sender.TinyId!
            },
            Content = CqCodeParser.ParseCqMessage(e.Message!)
        }));
    }

    public override async Task<string?> SendMessage(MessageSourceType sourceType, string? channelId, string? userId,
        MessageContent content, string? guildId = null)
    {
        if (sourceType == MessageSourceType.Channel)
            return await Internal.SendGuildChannelMessage(guildId!, channelId!, content);

        _mainBot.Logger.Warning("qqguild 平台暂不支持发送频道私聊消息。");
        return null;
    }

    public override async Task<string?> SendChannelMessage(string channelId, MessageContent content,
        string? guildId = null)
    {
        return await Internal.SendGuildChannelMessage(guildId!, channelId, content);
    }

    public override async Task<User?> GetSelf()
    {
        var self = await Internal.GetGuildServiceProfile();
        return new User
        {
            Name = self.Nickname!,
            UserId = self.TinyId!,
            AvatarUrl = self.AvatarUrl
        };
    }

    public override Task<User?> GetUser(string userId, string? guildId = null)
    {
        _mainBot.Logger.Warning(
            $"qqguild 平台暂不支持 {nameof(GetUser)} 方法。若您需要获取频道成员信息，请使用 {nameof(GetGuildMember)} 方法代替。");
        return Task.FromResult<User?>(null);
    }

    public override Task<IEnumerable<User>> GetFriendList()
    {
        return Task.FromResult<IEnumerable<User>>(Array.Empty<User>());
    }

    public override async Task<Guild?> GetGuild(string guildId)
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

    public override async Task<IEnumerable<Guild>> GetGuildList()
    {
        return (await Internal.GetGuildList()).Select(g => new Guild
        {
            Id = g.GuildId!,
            Name = g.GuildName!
        });
    }

    public override async Task<GuildMember?> GetGuildMember(string guildId, string userId)
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

    public override async Task<IEnumerable<GuildMember>> GetGuildMemberList(string guildId)
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

    public override async Task<Channel?> GetChannel(string channelId, string? guildId = null)
    {
        return (await GetChannelList(guildId!)).FirstOrDefault(c => c.Id == channelId);
    }

    public override async Task<IEnumerable<Channel>> GetChannelList(string guildId)
    {
        return (await Internal.GetGuildChannelList(guildId)).Select(c => new Channel
        {
            Id = c.ChannelId!,
            Name = c.ChannelName!
        });
    }
}