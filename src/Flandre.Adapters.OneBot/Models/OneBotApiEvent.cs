using System.Text.Json.Serialization;

namespace Flandre.Adapters.OneBot.Models;

internal class OneBotApiEvent
{
    [JsonPropertyName("time")] public long Time { get; set; }

    [JsonPropertyName("self_id")] public long SelfId { get; set; }
}