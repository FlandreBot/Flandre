using System.Text.Json.Serialization;

#pragma warning disable CS8618

namespace Flandre.Adapters.OneBot.Models;

public class OneBotGroupMember
{
    [JsonPropertyName("group_id")]
    public long GroupId { get; set; }

    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    [JsonPropertyName("nickname")]
    public string Nickname { get; set; }

    [JsonPropertyName("card")]
    public string Card { get; set; }

    [JsonPropertyName("sex")]
    public string Sex { get; set; }

    [JsonPropertyName("age")]
    public int Age { get; set; }

    [JsonPropertyName("area")]
    public string Area { get; set; } = "";

    [JsonPropertyName("join_time")]
    public int JoinTime { get; set; }

    [JsonPropertyName("last_sent_time")]
    public int LastSentTime { get; set; }

    [JsonPropertyName("level")]
    public string Level { get; set; } = "";

    [JsonPropertyName("role")]
    public string Role { get; set; } = "";

    [JsonPropertyName("unfriendly")]
    public bool Unfriendly { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("title_expire_time")]
    public long TitleExpireTime { get; set; }

    [JsonPropertyName("card_changeable")]
    public bool CardChangeable { get; set; }

    [JsonPropertyName("shut_up_timestamp")]
    public long ShutUpTimestamp { get; set; }
}
