using System.Text.Json.Serialization;

namespace Flandre.Adapters.OneBot.Models;

public class OneBotGuildServiceProfile
{
    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("tiny_id")]
    public string? TinyId { get; set; }

    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }
}