using System.Text.Json;
using Flandre.Core.Common;
using Flandre.Core.Events;
using Flandre.Core.Messaging;
using Flandre.Core.Models;

namespace Flandre.Adapters.OneBot;

public abstract class OneBotBot : Bot
{
    /// <summary>
    /// Bot 平台名称，值为 onebot
    /// </summary>
    public override string Platform => "onebot";

    /// <inheritdoc />
    public override string SelfId { get; }

    internal readonly OneBotGuildBot GuildBot;

    /// <summary>
    /// 内部 Bot，包含大量 OneBot 平台专属方法
    /// </summary>
    public OneBotInternalBot Internal { get; }

    internal OneBotBot(string selfId)
    {
        SelfId = selfId;
        Internal = new OneBotInternalBot(this);
        GuildBot = new OneBotGuildBot(this);
    }

    internal abstract Task<JsonElement> SendApiRequest(string action, object? @params = null);

    #region 核心方法

    /// <inheritdoc />
    public override async Task<string?> SendChannelMessage(string channelId, MessageContent content,
        string? guildId = null)
    {
        return (await Internal.SendGroupMessage(long.Parse(channelId), content)).ToString();
    }

    /// <inheritdoc />
    public override async Task<string?> SendPrivateMessage(string userId, MessageContent content)
    {
        return (await Internal.SendPrivateMessage(long.Parse(userId), content)).ToString();
    }

    /// <inheritdoc />
    public override Task DeleteMessage(string messageId)
    {
        return Internal.DeleteMessage(int.Parse(messageId));
    }

    /// <inheritdoc />
    public override async Task<User?> GetSelf()
    {
        var user = await Internal.GetLoginInfo();
        return new User
        {
            Name = user.Nickname,
            UserId = user.UserId.ToString(),
            AvatarUrl = OneBotUtils.GetUserAvatar(user.UserId)
        };
    }

    /// <inheritdoc />
    public override async Task<User?> GetUser(string userId, string? guildId = null)
    {
        var user = await Internal.GetStrangerInfo(long.Parse(userId));
        return new User
        {
            Name = user.Nickname,
            UserId = user.UserId.ToString(),
            AvatarUrl = OneBotUtils.GetUserAvatar(user.UserId)
        };
    }

    /// <inheritdoc />
    public override async Task<IEnumerable<User>> GetFriendList()
    {
        var list = await Internal.GetFriendList();
        return list.Select(f => new User
        {
            Name = f.Nickname,
            Nickname = f.Remark,
            UserId = f.UserId.ToString(),
            AvatarUrl = OneBotUtils.GetUserAvatar(f.UserId)
        });
    }

    /// <inheritdoc />
    public override async Task<Guild?> GetGuild(string guildId)
    {
        try
        {
            var group = await Internal.GetGroupInfo(long.Parse(guildId));
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

    /// <inheritdoc />
    public override async Task<IEnumerable<Guild>> GetGuildList()
    {
        var list = await Internal.GetGroupList();
        return list.Select(g => new Guild
        {
            Id = g.GroupId.ToString(),
            Name = g.GroupName
        });
    }

    /// <inheritdoc />
    public override async Task<GuildMember?> GetGuildMember(string guildId, string userId)
    {
        try
        {
            var member = await Internal.GetGroupMemberInfo(long.Parse(guildId), long.Parse(userId));
            return new GuildMember
            {
                Name = member.Nickname,
                Nickname = member.Card,
                UserId = member.UserId.ToString(),
                AvatarUrl = OneBotUtils.GetUserAvatar(member.UserId),
                Roles = new List<string> { member.Role }
            };
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public override async Task<IEnumerable<GuildMember>> GetGuildMemberList(string guildId)
    {
        var list = await Internal.GetGroupMemberList(long.Parse(guildId));
        return list.Select(member => new GuildMember
        {
            Name = member.Nickname,
            Nickname = member.Card,
            UserId = member.UserId.ToString(),
            AvatarUrl = OneBotUtils.GetUserAvatar(member.UserId),
            Roles = new List<string> { member.Role }
        });
    }

    /// <inheritdoc />
    public override async Task<Channel?> GetChannel(string channelId, string? guildId = null)
    {
        try
        {
            var group = await Internal.GetGroupInfo(long.Parse(channelId));
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

    /// <inheritdoc />
    public override async Task<IEnumerable<Channel>> GetChannelList(string guildId)
    {
        var list = await Internal.GetGroupList();
        return list.Select(c => new Channel
        {
            Id = c.GroupId.ToString(),
            Name = c.GroupName
        });
    }

    /// <inheritdoc />
    public override async Task HandleGuildInvitation(BotGuildInvitedEvent e, bool approve, string? comment = null)
    {
        await Internal.SetGroupAddRequest(e.EventMessage?.ToString()!, "invite", approve, comment ?? "");
    }

    /// <inheritdoc />
    public override async Task HandleGuildJoinRequest(BotGuildJoinRequestedEvent e, bool approve,
        string? comment = null)
    {
        await Internal.SetGroupAddRequest(e.EventMessage?.ToString()!, "add", approve, comment ?? "");
    }

    /// <inheritdoc />
    public override async Task HandleFriendRequest(BotFriendRequestedEvent e, bool approve, string? comment = null)
    {
        await Internal.SetFriendAddRequest(e.EventMessage?.ToString()!, approve, comment ?? "");
    }

    #endregion
}
