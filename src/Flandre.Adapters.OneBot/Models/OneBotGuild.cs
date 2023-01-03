using System.Text.Json.Serialization;

namespace Flandre.Adapters.OneBot.Models;

public class OneBotGuild
{
    [JsonPropertyName("guild_id")]
    public string? GuildId { get; set; }

    [JsonPropertyName("guild_name")]
    public string? GuildName { get; set; }

    [JsonPropertyName("guild_display_id")]
    public string? GuildDisplayId { get; set; }
}