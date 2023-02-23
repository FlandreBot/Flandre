using System.Text.Json;
using Flandre.Adapters.OneBot.Models;
using Flandre.Core.Messaging;

// ReSharper disable RedundantAnonymousTypePropertyName

namespace Flandre.Adapters.OneBot;

public class OneBotInternalBot
{
    private readonly OneBotBot _parent;

    internal OneBotInternalBot(OneBotBot parent)
    {
        _parent = parent;
    }

    public async Task<int> SendPrivateMessage(long userId, string message, bool autoEscape = false)
    {
        var resp = await _parent.SendApiRequest("send_private_msg",
            new
            {
                user_id = userId,
                message = message,
                auto_escape = autoEscape
            });
        return resp.GetProperty("message_id").GetInt32();
    }

    public Task<int> SendPrivateMessage(long userId, MessageContent content, bool autoEscape = false)
    {
        return SendPrivateMessage(userId, content.ToCqMessage(), autoEscape);
    }

    public async Task<int> SendGroupMessage(long groupId, string message, bool autoEscape = false)
    {
        var resp = await _parent.SendApiRequest("send_group_msg",
            new
            {
                group_id = groupId,
                message = message,
                auto_escape = autoEscape
            });
        return resp.GetProperty("message_id").GetInt32();
    }

    public Task<int> SendGroupMessage(long groupId, MessageContent content, bool autoEscape = false)
    {
        return SendGroupMessage(groupId, content.ToCqMessage(), autoEscape);
    }

    public async Task<int> SendMessage(long? userId, long? groupId, string message, bool autoEscape = false)
    {
        var resp = await _parent.SendApiRequest("send_msg",
            new
            {
                message_type = userId is not null ? "private" : "group",
                user_id = userId,
                group_id = groupId,
                message = message,
                auto_escape = autoEscape
            });
        return resp.GetProperty("message_id").GetInt32();
    }

    public Task<int> SendMessage(long? userId, long? groupId, MessageContent content, bool autoEscape = false)
    {
        return SendMessage(userId, groupId, content.ToCqMessage(), autoEscape);
    }

    public async Task DeleteMessage(int messageId)
    {
        await _parent.SendApiRequest("delete_msg",
            new { message_id = messageId });
    }

    public async Task<OneBotMessage> GetMessage(int messageId)
    {
        return (await _parent.SendApiRequest("get_msg",
                new { message_id = messageId }))
            .Deserialize<OneBotMessage>()!;
    }

    public async Task MarkMessageAsRead(int messageId)
    {
        await _parent.SendApiRequest("mark_msg_as_read",
            new { message_id = messageId });
    }

    public async Task SetGroupKick(long groupId, long userId, bool rejectAddRequest = false)
    {
        await _parent.SendApiRequest("set_group_kick",
            new
            {
                group_id = groupId,
                user_id = userId,
                reject_add_request = rejectAddRequest
            });
    }

    public async Task SetGroupBan(long groupId, long userId, int duration = 1800)
    {
        await _parent.SendApiRequest("set_group_ban", new
        {
            group_id = groupId,
            user_id = userId,
            duration = duration
        });
    }

    public async Task SetGroupWholeBan(long groupId, bool enable = true)
    {
        await _parent.SendApiRequest("set_group_whole_ban", new
        {
            group_id = groupId,
            enable = enable
        });
    }

    public async Task SetGroupAdmin(long groupId, long userId, bool enable = true)
    {
        await _parent.SendApiRequest("set_group_admin", new
        {
            group_id = groupId,
            user_id = userId,
            enable = enable
        });
    }

    public async Task SetGroupCard(long groupId, long userId, string card = "")
    {
        await _parent.SendApiRequest("set_group_card", new
        {
            group_id = groupId,
            user_id = userId,
            card = card
        });
    }

    public async Task SetGroupName(long groupId, string groupName)
    {
        await _parent.SendApiRequest("set_group_name", new
        {
            group_id = groupId,
            group_name = groupName
        });
    }

    public async Task SetGroupLeave(long groupId, bool isDismiss = false)
    {
        await _parent.SendApiRequest("set_group_leave", new
        {
            group_id = groupId,
            is_dismiss = isDismiss
        });
    }

    public async Task SetGroupSpecialTitle(long groupId, long userId, string specialTitle = "", int duration = -1)
    {
        await _parent.SendApiRequest("set_group_special_title", new
        {
            group_id = groupId,
            user_id = userId,
            special_title = specialTitle,
            duration = duration
        });
    }

    public async Task SendGroupSign(long groupId)
    {
        await _parent.SendApiRequest("send_group_sign", new { group_id = groupId });
    }

    public async Task SetFriendAddRequest(string flag, bool approve = true, string remark = "")
    {
        await _parent.SendApiRequest("set_friend_add_request", new
        {
            flag,
            approve,
            remark
        });
    }

    public async Task SetGroupAddRequest(string flag, string subType, bool approve = true, string reason = "")
    {
        await _parent.SendApiRequest("set_group_add_request", new
        {
            flag,
            approve,
            reason,
            sub_type = subType
        });
    }

    public async Task<OneBotLoginInfo> GetLoginInfo()
    {
        return (await _parent.SendApiRequest("get_login_info")).Deserialize<OneBotLoginInfo>()!;
    }

    public async Task SetQqProfile(string nickname, string company, string email, string college,
        string personalNote)
    {
        await _parent.SendApiRequest("set_qq_profile", new
        {
            nickname,
            company,
            email,
            college,
            personal_note = personalNote
        });
    }

    public async Task<OneBotUser> GetStrangerInfo(long userId, bool noCache = false)
    {
        return (await _parent.SendApiRequest("get_stranger_info", new
            {
                user_id = userId,
                no_cache = noCache
            }))
            .Deserialize<OneBotUser>()!;
    }

    public async Task<OneBotFriend[]> GetFriendList()
    {
        return (await _parent.SendApiRequest("get_friend_list"))
            .Deserialize<OneBotFriend[]>()!;
    }

    public async Task DeleteFriend(long friendId)
    {
        await _parent.SendApiRequest("delete_friend", new { friend_id = friendId });
    }

    public async Task<OneBotGroup> GetGroupInfo(long groupId, bool noCache = false)
    {
        return (await _parent.SendApiRequest("get_group_info", new
            {
                group_id = groupId,
                no_cache = noCache
            }))
            .Deserialize<OneBotGroup>()!;
    }

    public async Task<OneBotGroup[]> GetGroupList()
    {
        return (await _parent.SendApiRequest("get_group_list"))
            .Deserialize<OneBotGroup[]>()!;
    }

    public async Task<OneBotGroupMember> GetGroupMemberInfo(long groupId, long userId, bool noCache = false)
    {
        return (await _parent.SendApiRequest("get_group_member_info", new
        {
            group_id = groupId,
            user_id = userId,
            no_cache = noCache
        })).Deserialize<OneBotGroupMember>()!;
    }

    public async Task<OneBotGroupMember[]> GetGroupMemberList(long groupId, bool noCache = false)
    {
        return (await _parent.SendApiRequest("get_group_member_list", new
            {
                group_id = groupId,
                no_cache = noCache
            }))
            .Deserialize<OneBotGroupMember[]>()!;
    }
}
