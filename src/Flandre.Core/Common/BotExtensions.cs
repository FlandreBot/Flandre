using Flandre.Core.Messaging;

namespace Flandre.Core.Common;

/// <summary>
/// 机器人扩展方法
/// </summary>
public static class BotExtensions
{
    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="bot">发送消息的机器人</param>
    /// <param name="environment">消息类型</param>
    /// <param name="channelId">频道 ID</param>
    /// <param name="userId">用户 ID</param>
    /// <param name="content">消息内容</param>
    /// <param name="guildId">群组 ID</param>
    public static Task<string?> SendMessageAsync(this Bot bot, MessageEnvironment environment, string? channelId,
        string? userId,
        MessageContent content,
        string? guildId = null)
    {
        return environment switch
        {
            MessageEnvironment.Channel => bot.SendChannelMessageAsync(channelId!, content, guildId),
            MessageEnvironment.Private => bot.SendPrivateMessageAsync(userId!, content),
            _ => Task.FromResult<string?>(null)
        };
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="bot">发送消息的机器人</param>
    /// <param name="message">消息对象</param>
    /// <param name="contentOverride">覆盖消息对象的内容，可选</param>
    public static Task<string?> SendMessageAsync(this Bot bot, Message message, MessageContent? contentOverride = null)
    {
        return SendMessageAsync(bot, message.Environment, message.ChannelId, message.Sender.UserId,
            contentOverride ?? message.Content, message.GuildId);
    }
}
