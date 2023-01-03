using System.Text.Json.Serialization;

#pragma warning disable CS8618

namespace Flandre.Adapters.OneBot.Models;

internal class OneBotApiMessageEvent : OneBotApiEvent
{
    [JsonPropertyName("sub_type")]
    public string SubType { get; set; }

    [JsonPropertyName("message_id")]
    public int MessageId { get; set; }

    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    // [JsonPropertyName("message")]
    // public string Message { get; set; }

    [JsonPropertyName("raw_message")]
    public string RawMessage { get; set; }

    // [JsonPropertyName("font")]
    // public string Font { get; set; }

    [JsonPropertyName("sender")]
    public OneBotMessageSender Sender { get; set; }

    [JsonPropertyName("message_type")]
    public string MessageType { get; set; }

    // 私聊消息专有
    // [JsonPropertyName("temp_source")]
    // public int? TempSource { get; set; }

    // 以下为群消息专有
    [JsonPropertyName("group_id")]
    public long? GroupId { get; set; }

    [JsonPropertyName("anonymous")]
    public object? Anonymous { get; set; }
}