using Flandre.Core.Common;

namespace Flandre.Core.Messaging;

/// <summary>
/// 消息上下文
/// </summary>
public class MessageContext : BotContext
{
    /// <summary>
    /// 当前消息
    /// </summary>
    public Message Message { get; init; }

    /// <summary>
    /// 构造消息上下文
    /// </summary>
    /// <param name="bot">bot 实例</param>
    /// <param name="message">消息</param>
    public MessageContext(Bot bot, Message message)
        : base(bot)
    {
        Message = message;
    }

    /// <summary>
    /// 用户 ID，等同于 <see cref="Message"/>.Sender.UserId
    /// </summary>
    public string UserId => Message.Sender.UserId;

    /// <summary>
    /// 群组 ID，等同于 <see cref="Message"/>.GuildId
    /// </summary>
    public string? GuildId => Message.GuildId;

    /// <summary>
    /// 频道 ID，等同于 <see cref="Message"/>.ChannelId
    /// </summary>
    public string? ChannelId => Message.ChannelId;
}
