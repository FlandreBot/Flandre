using System.Text.Json;
using Flandre.Core.Common;
using Flandre.Core.Events.Bot;
using Flandre.Core.Messaging;
using Flandre.Core.Models;

namespace Flandre.Adapters.OneBot;

public abstract partial class OneBotBot : IBot
{
    protected abstract Task<JsonElement> SendApiRequest(string action, object? @params = null);

    #region 核心方法

    public abstract Task Start();

    public abstract Task Stop();

    public Task<string?> SendMessage(MessageSourceType sourceType, string? channelId, string? userId,
        MessageContent content)
    {
        return sourceType switch
        {
            MessageSourceType.Channel => SendChannelMessage(channelId!, content),
            MessageSourceType.Private => SendPrivateMessage(userId!, content),
            _ => Task.FromResult<string?>(null)
        };
    }

    public Task<string?> SendMessage(Message message, MessageContent? contentOverride = null)
    {
        return SendMessage(message.SourceType, message.ChannelId, message.Sender.Id,
            contentOverride ?? message.Content);
    }

    public async Task<string?> SendChannelMessage(string channelId, MessageContent content)
    {
        return (await SendGroupMessage(long.Parse(channelId), content)).ToString();
    }

    public async Task<string?> SendPrivateMessage(string userId, MessageContent content)
    {
        return (await SendPrivateMessage(long.Parse(userId), content)).ToString();
    }

    public Task DeleteMessage(string messageId)
    {
        return DeleteMessage(int.Parse(messageId));
    }

    public async Task<User> GetSelf()
    {
        var user = await GetLoginInfo();
        return new User
        {
            Name = user.Nickname,
            Id = user.UserId.ToString(),
            AvatarUrl = OneBotUtils.GetUserAvatar(user.UserId)
        };
    }

    public async Task<User?> GetUser(string userId)
    {
        var user = await GetStrangerInfo(long.Parse(userId));
        return new User
        {
            Name = user.Nickname,
            Id = user.UserId.ToString(),
            AvatarUrl = OneBotUtils.GetUserAvatar(user.UserId)
        };
    }

    public async Task<IEnumerable<User>> GetFriendList()
    {
        var list = await GetOneBotFriendList();
        return list.Select(f => new User
        {
            Name = f.Nickname,
            Nickname = f.Remark,
            Id = f.UserId.ToString(),
            AvatarUrl = OneBotUtils.GetUserAvatar(f.UserId)
        });
    }

    public async Task<Guild?> GetGuild(string guildId)
    {
        try
        {
            var group = await GetGroupInfo(long.Parse(guildId));
            return new Guild
            {
                Id = group.GroupId.ToString(),
                Name = group.GroupName
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<IEnumerable<Guild>> GetGuildList()
    {
        var list = await GetGroupList();
        return list.Select(g => new Guild
        {
            Id = g.GroupId.ToString(),
            Name = g.GroupName
        });
    }

    public async Task<GuildMember?> GetGuildMember(string guildId, string userId)
    {
        try
        {
            var member = await GetGroupMemberInfo(long.Parse(guildId), long.Parse(userId));
            return new GuildMember
            {
                Name = member.Nickname,
                Nickname = member.Card,
                Id = member.UserId.ToString(),
                AvatarUrl = OneBotUtils.GetUserAvatar(member.UserId),
                Roles = new List<string> { member.Role }
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<IEnumerable<GuildMember>> GetGuildMemberList(string guildId)
    {
        var list = await GetGroupMemberList(long.Parse(guildId));
        return list.Select(member => new GuildMember
        {
            Name = member.Nickname,
            Nickname = member.Card,
            Id = member.UserId.ToString(),
            AvatarUrl = OneBotUtils.GetUserAvatar(member.UserId),
            Roles = new List<string> { member.Role }
        });
    }

    public async Task<Channel?> GetChannel(string channelId)
    {
        try
        {
            var group = await GetGroupInfo(long.Parse(channelId));
            return new Channel
            {
                Id = group.GroupId.ToString(),
                Name = group.GroupName
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<IEnumerable<Channel>> GetChannelList()
    {
        var list = await GetGroupList();
        return list.Select(c => new Channel
        {
            Id = c.GroupId.ToString(),
            Name = c.GroupName
        });
    }

    public abstract event IBot.BotEventHandler<BotMessageReceivedEvent>? OnMessageReceived;

    public abstract event IBot.BotEventHandler<BotGuildInvitedEvent>? OnGuildInvited;

    public abstract event IBot.BotEventHandler<BotGuildRequestedEvent>? OnGuildRequested;

    public abstract event IBot.BotEventHandler<BotFriendRequestedEvent>? OnFriendRequested;

    public async Task HandleGuildInvitation(BotGuildInvitedEvent e, bool approve, string? comment = null)
    {
        await SetGroupAddRequest(e.EventMessage?.ToString()!, "invite", approve, comment ?? "");
    }

    public async Task HandleGuildRequest(BotGuildRequestedEvent e, bool approve, string? comment = null)
    {
        await SetGroupAddRequest(e.EventMessage?.ToString()!, "add", approve, comment ?? "");
    }

    public async Task HandleFriendRequest(BotFriendRequestedEvent e, bool approve, string? comment = null)
    {
        await SetFriendAddRequest(e.EventMessage?.ToString()!, approve, comment ?? "");
    }

    #endregion
}

public class OneBotBotConfig : BotConfig
{
    /// <summary>
    /// 连接 OneBot 服务端使用的协议。目前仅支持 "websocket" 或 "ws"。
    /// </summary>
    public string Protocol { get; set; } = "";

    /// <summary>
    /// 和 OneBot 服务端通信时使用的终结点。
    /// </summary>
    public string? Endpoint { get; set; } = null;

    /// <summary>
    /// WebSocket 服务端重连等待事件，单位为秒。
    /// </summary>
    public int WebSocketReconnectTimeout { get; set; } = 10;
}