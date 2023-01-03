using System.Text.Json.Serialization;

namespace Flandre.Adapters.OneBot.Models;

public class OneBotGuildMemberProfile : OneBotGuildServiceProfile
{
    [JsonPropertyName("join_time")]
    public long JoinTime { get; set; }

    [JsonPropertyName("roles")]
    public RoleInfo[]? Roles { get; set; }

    public class RoleInfo
    {
        [JsonPropertyName("role_id")]
        public string? RoleId { get; set; }

        [JsonPropertyName("role_name")]
        public string? RoleName { get; set; }
    }
}