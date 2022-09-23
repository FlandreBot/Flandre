using Flandre.Core.Events.Bot;
using Flandre.Core.Messaging;
using Flandre.Core.Models;

namespace Flandre.Core.Common;

/// <summary>
/// bot 抽象接口
/// </summary>
public interface IBot
{
    #region 生命周期

    /// <summary>
    /// 启动 Bot 实例
    /// </summary>
    Task Start();

    /// <summary>
    /// 停止 Bot 实例
    /// </summary>
    Task Stop();

    #endregion 生命周期

    #region 发送消息

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="sourceType">消息类型</param>
    /// <param name="guildId">Guild ID</param>
    /// <param name="channelId">Channel ID</param>
    /// <param name="userId">用户 ID</param>
    /// <param name="content">消息内容</param>
    Task SendMessage(MessageSourceType sourceType, string guildId, string channelId, string userId,
        MessageContent content);

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="message">消息对象</param>
    Task SendMessage(Message message);

    /// <summary>
    /// 发送频道 (Channel) 消息
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="channelId">Channel ID</param>
    /// <param name="content">消息内容</param>
    Task SendChannelMessage(string guildId, string channelId, MessageContent content);

    /// <summary>
    /// 发送私聊消息
    /// </summary>
    /// <param name="userId">用户 ID</param>
    /// <param name="content">消息内容</param>
    Task SendPrivateMessage(string userId, MessageContent content);

    #endregion 发送消息

    #region 用户相关

    /// <summary>
    /// 获取自身信息
    /// </summary>
    Task<User> GetSelf();

    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <param name="userId">用户 ID</param>
    Task<User?> GetUser(string userId);

    /// <summary>
    /// 获取好友列表
    /// </summary>
    Task<IEnumerable<User>> GetFriendList();

    #endregion 用户相关

    #region Guild 相关

    /// <summary>
    /// 获取 Guild 信息
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    Task<Guild?> GetGuild(string guildId);

    /// <summary>
    /// 获取 Guild 列表
    /// </summary>
    Task<IEnumerable<Guild>> GetGuildList();

    /// <summary>
    /// 获取 Guild 成员
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="userId">用户 ID</param>
    Task<GuildMember?> GetGuildMember(string guildId, string userId);

    /// <summary>
    /// 获取 Guild 成员列表
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    Task<IEnumerable<GuildMember>> GetGuildMemberList(string guildId);

    #endregion Guild 相关

    #region Channel 相关

    /// <summary>
    /// 获取 Channel 信息
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    Task<Channel?> GetChannel(string channelId);

    /// <summary>
    /// 获取 Channel 列表
    /// </summary>
    Task<IEnumerable<Channel>> GetChannelList();

    #endregion Channel 相关

    #region 事件相关

    /// <summary>
    /// Bot 事件委托
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    delegate void BotEventHandler<in TEvent>(IBot bot, TEvent e);

    /// <summary>
    /// 收到消息
    /// </summary>
    event BotEventHandler<BotMessageReceivedEvent> OnMessageReceived;

    /// <summary>
    /// 收到拉群申请
    /// </summary>
    event BotEventHandler<BotGuildInvitedEvent> OnGuildInvited;

    /// <summary>
    /// 收到加群申请
    /// </summary>
    event BotEventHandler<BotGuildRequestedEvent> OnGuildRequested;

    /// <summary>
    /// 收到好友申请
    /// </summary>
    event BotEventHandler<BotFriendRequestedEvent> OnFriendRequested;

    #endregion 事件相关
}

/// <summary>
/// bot 状态
/// </summary>
public enum BotStatus
{
    /// <summary>
    /// 在线
    /// </summary>
    Online,

    /// <summary>
    /// 离线
    /// </summary>
    Offline
}

/// <summary>
/// bot 基本配置
/// </summary>
public class BotConfig
{
    /// <summary>
    /// 自身 ID
    /// </summary>
    public string SelfId { get; set; } = "";
}