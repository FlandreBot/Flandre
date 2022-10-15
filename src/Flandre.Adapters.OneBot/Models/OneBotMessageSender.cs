using System.Text.Json.Serialization;

#pragma warning disable CS8618

namespace Flandre.Adapters.OneBot.Models;

public class OneBotMessageSender
{
    [JsonPropertyName("user_id")] public long UserId { get; set; }

    [JsonPropertyName("nickname")] public string Nickname { get; set; }

    [JsonPropertyName("sex")] public string Sex { get; set; }

    [JsonPropertyName("age")] public int Age { get; set; }

    // 以下为群聊专属

    [JsonPropertyName("card")] public string? Card { get; set; }

    [JsonPropertyName("area")] public string? Area { get; set; }

    [JsonPropertyName("level")] public string? Level { get; set; }

    [JsonPropertyName("role")] public string? Role { get; set; }

    [JsonPropertyName("title")] public string? Title { get; set; }

    // 以下为频道专属
    [JsonPropertyName("tiny_id")] public string? TinyId { get; set; }
}