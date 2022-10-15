using System.Text.Json;
using Flandre.Adapters.OneBot.Models;
using Flandre.Core.Messaging;

namespace Flandre.Adapters.OneBot;

public class OneBotGuildInternalBot
{
    private readonly OneBotBot _mainBot;

    internal OneBotGuildInternalBot(OneBotBot mainBot)
    {
        _mainBot = mainBot;
    }

    public async Task<OneBotGuildServiceProfile> GetGuildServiceProfile()
    {
        return (await _mainBot.SendApiRequest("get_guild_service_profile"))
            .Deserialize<OneBotGuildServiceProfile>()!;
    }

    public async Task<OneBotGuild[]> GetGuildList()
    {
        var list = await _mainBot.SendApiRequest("get_guild_list");
        if (list.ValueKind == JsonValueKind.Null)
            return Array.Empty<OneBotGuild>();
        return list.Deserialize<OneBotGuild[]>()!;
    }

    public async Task<OneBotGuildMeta> GetGuildMetaByGuest(string guildId)
    {
        return (await _mainBot.SendApiRequest("get_guild_service_profile", new
        {
            guild_id = guildId
        })).Deserialize<OneBotGuildMeta>()!;
    }

    public async Task<OneBotGuildChannel[]> GetGuildChannelList(string guildId, bool noCache = false)
    {
        return (await _mainBot.SendApiRequest("get_guild_channel_list", new
        {
            guild_id = guildId,
            no_cache = noCache
        })).Deserialize<OneBotGuildChannel[]>()!;
    }

    public async Task<OneBotGuildMemberListResponse> GetGuildMemberList(string guildId, string nextToken = "")
    {
        return (await _mainBot.SendApiRequest("get_guild_member_list", new
        {
            guild_id = guildId,
            next_token = nextToken
        })).Deserialize<OneBotGuildMemberListResponse>()!;
    }

    public async Task<OneBotGuildMemberProfile> GetGuildMemberProfile(string guildId, string userId)
    {
        return (await _mainBot.SendApiRequest("get_guild_member_profile", new
        {
            guild_id = guildId,
            user_id = userId
        })).Deserialize<OneBotGuildMemberProfile>()!;
    }

    public async Task<string> SendGuildChannelMessage(string guildId, string channelId, string message)
    {
        return (await _mainBot.SendApiRequest("send_guild_channel_msg", new
        {
            guild_id = guildId,
            channel_id = channelId,
            message
        })).GetProperty("message_id").ToString();
    }

    public Task<string> SendGuildChannelMessage(string guildId, string channelId, MessageContent content)
    {
        return SendGuildChannelMessage(guildId, channelId, content.ToCqMessage());
    }
}