using System.Text.Json.Serialization;

#pragma warning disable CS8618

namespace Flandre.Adapters.OneBot.Models;

public class OneBotGroup
{
    [JsonPropertyName("group_id")] public long GroupId { get; set; }

    [JsonPropertyName("group_name")] public string GroupName { get; set; }

    [JsonPropertyName("group_memo")] public string GroupMemo { get; set; }

    /// <remarks>若 Bot 未加入群，该项将会为 0。</remarks>
    [JsonPropertyName("group_create_time")]
    public uint GroupCreateTime { get; set; }

    /// <remarks>若 Bot 未加入群，该项将会为 0。</remarks>
    [JsonPropertyName("group_level")]
    public uint GroupLevel { get; set; }

    /// <remarks>若 Bot 未加入群，该项将会为 0。</remarks>
    [JsonPropertyName("member_count")]
    public int MemberCount { get; set; }

    /// <remarks>若 Bot 未加入群，该项将会为 0。</remarks>
    [JsonPropertyName("max_member_count")]
    public int MaxMemberCount { get; set; }
}