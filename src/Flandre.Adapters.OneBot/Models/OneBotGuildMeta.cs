using System.Text.Json.Serialization;

namespace Flandre.Adapters.OneBot.Models;

public class OneBotGuildMeta
{
    [JsonPropertyName("guild_id")]
    public string? GuildId { get; set; }

    [JsonPropertyName("guild_name")]
    public string? GuildName { get; set; }

    [JsonPropertyName("guild_profile")]
    public string? GuildProfile { get; set; }

    [JsonPropertyName("create_time")]
    public long CreateTime { get; set; }

    [JsonPropertyName("max_member_count")]
    public long MaxMemberCount { get; set; }

    [JsonPropertyName("max_robot_count")]
    public long MaxRobotCount { get; set; }

    [JsonPropertyName("max_admin_count")]
    public long MaxAdminCount { get; set; }

    [JsonPropertyName("member_count")]
    public long MemberCount { get; set; }

    [JsonPropertyName("owner_id")]
    public string? OwnerId { get; set; }
}
