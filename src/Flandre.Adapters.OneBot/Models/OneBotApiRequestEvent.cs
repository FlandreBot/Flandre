using System.Text.Json.Serialization;

#pragma warning disable CS8618

namespace Flandre.Adapters.OneBot.Models;

internal class OneBotApiRequestEvent : OneBotApiEvent
{
    [JsonPropertyName("request_type")] public string RequestType { get; set; }

    [JsonPropertyName("flag")] public string Flag { get; set; }

    [JsonPropertyName("comment")] public string Comment { get; set; }

    [JsonPropertyName("user_id")] public long UserId { get; set; }

    [JsonPropertyName("sub_type")] public string? SubType { get; set; }

    [JsonPropertyName("group_id")] public long? GroupId { get; set; }
}