using System.Text.Json.Serialization;

namespace Flandre.Adapters.OneBot.Models;

#pragma warning disable CS8618

public class OneBotUser
{
    [JsonPropertyName("user_id")] public long UserId { get; set; }

    [JsonPropertyName("nickname")] public string Nickname { get; set; }

    [JsonPropertyName("sex")] public string Sex { get; set; }

    [JsonPropertyName("age")] public int Age { get; set; }

    [JsonPropertyName("qid")] public string Qid { get; set; }

    [JsonPropertyName("level")] public int Level { get; set; }

    [JsonPropertyName("login_days")] public int LoginDays { get; set; }
}