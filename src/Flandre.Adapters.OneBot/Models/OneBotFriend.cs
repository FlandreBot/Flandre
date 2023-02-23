using System.Text.Json.Serialization;

#pragma warning disable CS8618

namespace Flandre.Adapters.OneBot.Models;

public class OneBotFriend
{
    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    [JsonPropertyName("nickname")]
    public string Nickname { get; set; }

    [JsonPropertyName("remark")]
    public string Remark { get; set; }
}
