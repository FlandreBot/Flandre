using Flandre.Core.Models;

namespace Flandre.Core.Messaging;

#pragma warning disable CS8618

/// <summary>
/// 消息结构
/// </summary>
public class Message
{
    /// <summary>
    /// 消息时间
    /// </summary>
    public DateTime Time { get; init; } = DateTime.Now;

    /// <summary>
    /// 消息来源类型
    /// </summary>
    public MessageSourceType SourceType { get; init; }

    /// <summary>
    /// 消息 ID
    /// </summary>
    public string MessageId { get; init; } = "";

    /// <summary>
    /// Guild ID
    /// </summary>
    public string GuildId { get; init; } = "";

    /// <summary>
    /// Channel ID
    /// </summary>
    public string ChannelId { get; init; } = "";

    /// <summary>
    /// 发送者信息
    /// </summary>
    public User Sender { get; init; }

    /// <summary>
    /// 消息内容
    /// </summary>
    public MessageContent Content { get; init; }

    /// <summary>
    /// 获取消息内容中的所有文本
    /// </summary>
    /// <returns></returns>
    public string GetText()
    {
        return Content.GetText();
    }
}

/// <summary>
/// 消息来源类型
/// </summary>
public enum MessageSourceType
{
    /// <summary>
    /// 来自 Channel
    /// </summary>
    Channel,

    /// <summary>
    /// 来自私聊
    /// </summary>
    Private
}