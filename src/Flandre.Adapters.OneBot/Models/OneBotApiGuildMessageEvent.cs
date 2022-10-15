using System.Text.Json.Serialization;

namespace Flandre.Adapters.OneBot.Models;

// 频道专有
internal class OneBotApiGuildMessageEvent : OneBotApiMessageEvent
{
    [JsonPropertyName("message_id")] public new string? MessageId { get; set; }

    [JsonPropertyName("user_id")] public new string? UserId { get; set; }

    [JsonPropertyName("guild_id")] public string? GuildId { get; set; }

    [JsonPropertyName("channel_id")] public string? ChannelId { get; set; }
}