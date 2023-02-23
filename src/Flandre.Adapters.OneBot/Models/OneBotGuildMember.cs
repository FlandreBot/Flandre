using System.Text.Json.Serialization;

namespace Flandre.Adapters.OneBot.Models;

public class OneBotGuildMemberListResponse
{
    [JsonPropertyName("members")]
    public OneBotGuildMember[]? Members { get; set; }

    [JsonPropertyName("finished")]
    public bool Finished { get; set; }

    [JsonPropertyName("next_token")]
    public string? NextToken { get; set; }
}

public class OneBotGuildMember
{
    [JsonPropertyName("tiny_id")]
    public string? TinyId { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("role_id")]
    public string? RoleId { get; set; }

    [JsonPropertyName("role_name")]
    public string? RoleName { get; set; }
}
