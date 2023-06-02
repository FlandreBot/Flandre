using Flandre.Core.Events;
using Flandre.Core.Messaging;
using Flandre.Core.Models;

namespace Flandre.Core.Common;

/// <summary>
/// 机器人
/// </summary>
public abstract partial class Bot
{
    /// <summary>
    /// Bot 所在平台名称
    /// </summary>
    public abstract string Platform { get; }

    /// <summary>
    /// Bot 自身 ID
    /// </summary>
    public abstract string SelfId { get; }

    /// <summary>
    /// 日志记录
    /// </summary>
    /// <param name="level">日志等级</param>
    /// <param name="message">日志消息</param>
    public void Log(BotLogLevel level, string message)
    {
        Logging?.Invoke(this, new BotLoggingEvent(level, message));
    }

    /// <summary>
    /// 启动 Bot 实例
    /// </summary>
    public virtual Task StartAsync() => Task.CompletedTask;

    /// <summary>
    /// 停止 Bot 实例
    /// </summary>
    public virtual Task StopAsync() => Task.CompletedTask;

    /// <summary>
    /// 该方法不受支持，发出警告
    /// </summary>
    /// <param name="method">方法名称</param>
    /// <returns>Task.CompletedTask</returns>
    protected Task LogNotSupportedAsync(string method)
    {
        Log(BotLogLevel.Debug, $"Platform {Platform} does not support method {method}.");
        return Task.CompletedTask;
    }

    /// <summary>
    /// 该方法不受支持，发出警告
    /// </summary>
    /// <param name="method">方法名称</param>
    /// <param name="result">返回值</param>
    /// <returns>Task.FromResult&lt;TResult&gt;(result)</returns>
    protected Task<TResult> LogNotSupportedAsync<TResult>(string method, TResult result)
    {
        Log(BotLogLevel.Debug, $"Platform {Platform} does not support method {method}.");
        return Task.FromResult(result);
    }

    /// <summary>
    /// 发送频道 (Channel) 消息
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="content">消息内容</param>
    /// <param name="guildId">群组 ID</param>
    public virtual Task<string?> SendChannelMessageAsync(string channelId, MessageContent content,
        string? guildId = null)
        => LogNotSupportedAsync<string?>(nameof(SendChannelMessageAsync), null);

    /// <summary>
    /// 发送私聊消息
    /// </summary>
    /// <param name="userId">用户 ID</param>
    /// <param name="content">消息内容</param>
    public virtual Task<string?> SendPrivateMessageAsync(string userId, MessageContent content)
        => LogNotSupportedAsync<string?>(nameof(SendPrivateMessageAsync), null);

    /// <summary>
    /// 删除（撤回）消息
    /// </summary>
    /// <param name="messageId">消息 ID</param>
    public virtual Task DeleteMessageAsync(string messageId)
        => LogNotSupportedAsync(nameof(DeleteMessageAsync));

    /// <summary>
    /// 获取自身信息
    /// </summary>
    public virtual Task<User?> GetSelfAsync()
        => LogNotSupportedAsync<User?>(nameof(GetSelfAsync), null);

    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <param name="userId">用户 ID</param>
    /// <param name="guildId">群组 ID</param>
    public virtual Task<User?> GetUserAsync(string userId, string? guildId = null)
        => LogNotSupportedAsync<User?>(nameof(GetUserAsync), null);

    /// <summary>
    /// 获取好友列表
    /// </summary>
    public virtual Task<IEnumerable<User>> GetFriendListAsync()
        => LogNotSupportedAsync<IEnumerable<User>>(nameof(GetFriendListAsync), Array.Empty<User>());

    /// <summary>
    /// 获取群组信息
    /// </summary>
    /// <param name="guildId">群组 ID</param>
    public virtual Task<Guild?> GetGuildAsync(string guildId)
        => LogNotSupportedAsync<Guild?>(nameof(GetGuildAsync), null);

    /// <summary>
    /// 获取群组列表
    /// </summary>
    public virtual Task<IEnumerable<Guild>> GetGuildListAsync()
        => LogNotSupportedAsync<IEnumerable<Guild>>(nameof(GetGuildListAsync), Array.Empty<Guild>());

    /// <summary>
    /// 获取群组成员信息
    /// </summary>
    /// <param name="guildId">群组 ID</param>
    /// <param name="userId">用户 ID</param>
    public virtual Task<GuildMember?> GetGuildMemberAsync(string guildId, string userId)
        => LogNotSupportedAsync<GuildMember?>(nameof(GetGuildMemberAsync), null);

    /// <summary>
    /// 获取群组成员列表
    /// </summary>
    /// <param name="guildId">群组 ID</param>
    public virtual Task<IEnumerable<GuildMember>> GetGuildMemberListAsync(string guildId)
        => LogNotSupportedAsync<IEnumerable<GuildMember>>(nameof(GetGuildListAsync), Array.Empty<GuildMember>());

    /// <summary>
    /// 获取频道信息
    /// </summary>
    /// <param name="channelId">频道 ID</param>
    /// <param name="guildId">群组 ID</param>
    public virtual Task<Channel?> GetChannelAsync(string channelId, string? guildId = null)
        => LogNotSupportedAsync<Channel?>(nameof(GetChannelAsync), null);

    /// <summary>
    /// 获取频道列表
    /// </summary>
    /// <param name="guildId">群组 ID</param>
    public virtual Task<IEnumerable<Channel>> GetChannelListAsync(string guildId)
        => LogNotSupportedAsync<IEnumerable<Channel>>(nameof(GetGuildListAsync), Array.Empty<Channel>());
}

/// <summary>
/// Bot 基本配置
/// </summary>
public class BotConfig
{
    /// <summary>
    /// 自身 ID
    /// </summary>
    public string SelfId { get; set; } = "";
}
