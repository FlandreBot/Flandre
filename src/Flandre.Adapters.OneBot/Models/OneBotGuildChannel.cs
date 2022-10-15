using System.Text.Json.Serialization;

namespace Flandre.Adapters.OneBot.Models;

public class OneBotGuildChannel
{
    [JsonPropertyName("owner_guild_id")] public string? OwnerGuildId { get; set; }

    [JsonPropertyName("channel_id")] public string? ChannelId { get; set; }

    [JsonPropertyName("channel_type")] public int ChannelType { get; set; }

    [JsonPropertyName("channel_name")] public string? ChannelName { get; set; }

    [JsonPropertyName("create_time")] public long CreateTime { get; set; }

    [JsonPropertyName("creator_tiny_id")] public string? CreatorTinyId { get; set; }

    [JsonPropertyName("talk_permission")] public int TalkPermission { get; set; }

    [JsonPropertyName("visible_type")] public int VisibleType { get; set; }

    [JsonPropertyName("current_slow_mode")]
    public int CurrentSlowMode { get; set; }

    [JsonPropertyName("slow_modes")] public SlowModeInfo[]? SlowModes { get; set; }

    public class SlowModeInfo
    {
        [JsonPropertyName("slow_mode_key")] public int SlowModeKey { get; set; }

        [JsonPropertyName("slow_mode_text")] public string? SlowModeText { get; set; }

        [JsonPropertyName("speak_frequency")] public int SpeekFrequency { get; set; }

        [JsonPropertyName("slow_mode_circle")] public int SlowModeCircle { get; set; }
    }
}