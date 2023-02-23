using System.Text.Json.Serialization;

#pragma warning disable CS8618

namespace Flandre.Adapters.OneBot.Models;

public class OneBotMessage
{
    [JsonPropertyName("message_id")]
    public int MessageId { get; set; }

    [JsonPropertyName("real_id")]
    public int RealId { get; set; }

    [JsonPropertyName("sender")]
    public OneBotMessageSender Sender { get; set; }

    [JsonPropertyName("time")]
    public int Time { get; set; }

    [JsonPropertyName("message")]
    public object Message { get; set; }

    [JsonPropertyName("raw_message")]
    public string RawMessage { get; set; }
}
