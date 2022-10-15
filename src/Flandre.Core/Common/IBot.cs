using Flandre.Core.Events.Bot;
using Flandre.Core.Messaging;
using Flandre.Core.Models;

namespace Flandre.Core.Common;

/// <summary>
/// bot 抽象接口
/// </summary>
public interface IBot
{
    /// <summary>
    /// Bot 平台名称
    /// </summary>
    string Platform { get; }

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

    #region 消息相关

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="sourceType">消息类型</param>
    /// <param name="channelId">Channel ID</param>
    /// <param name="userId">用户 ID</param>
    /// <param name="content">消息内容</param>
    /// <param name="guildId">群组 ID</param>
    Task<string?> SendMessage(MessageSourceType sourceType, string? channelId, string? userId,
        MessageContent content, string? guildId = null);

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="message">消息对象</param>
    /// <param name="contentOverride">覆盖消息对象的内容，可选</param>
    Task<string?> SendMessage(Message message, MessageContent? contentOverride = null);

    /// <summary>
    /// 发送频道 (Channel) 消息
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="content">消息内容</param>
    /// <param name="guildId">群组 ID</param>
    Task<string?> SendChannelMessage(string channelId, MessageContent content, string? guildId = null);

    /// <summary>
    /// 发送私聊消息
    /// </summary>
    /// <param name="userId">用户 ID</param>
    /// <param name="content">消息内容</param>
    Task<string?> SendPrivateMessage(string userId, MessageContent content);

    /// <summary>
    /// 删除（撤回）消息
    /// </summary>
    /// <param name="messageId">消息 ID</param>
    Task DeleteMessage(string messageId);

    #endregion 消息相关

    #region 用户相关

    /// <summary>
    /// 获取自身信息
    /// </summary>
    Task<User> GetSelf();

    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <param name="userId">用户 ID</param>
    /// <param name="guildId">群组 ID</param>
    Task<User?> GetUser(string userId, string? guildId = null);

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
    /// <param name="guildId">群组 ID</param>
    Task<Channel?> GetChannel(string channelId, string? guildId = null);

    /// <summary>
    /// 获取 Channel 列表
    /// </summary>
    /// <param name="guildId">群组 ID</param>
    Task<IEnumerable<Channel>> GetChannelList(string guildId);

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

    #region 事件处理

    /// <summary>
    /// 处理拉群邀请
    /// </summary>
    /// <param name="e">拉群邀请事件</param>
    /// <param name="approve">是否同意</param>
    /// <param name="comment">附加说明</param>
    Task HandleGuildInvitation(BotGuildInvitedEvent e, bool approve, string? comment = null);

    /// <summary>
    /// 处理加群申请
    /// </summary>
    /// <param name="e">加群申请事件</param>
    /// <param name="approve">是否同意</param>
    /// <param name="comment">附加说明</param>
    Task HandleGuildRequest(BotGuildRequestedEvent e, bool approve, string? comment = null);

    /// <summary>
    /// 处理好友申请
    /// </summary>
    /// <param name="e">好友申请事件</param>
    /// <param name="approve">是否同意</param>
    /// <param name="comment">附加说明</param>
    Task HandleFriendRequest(BotFriendRequestedEvent e, bool approve, string? comment = null);

    #endregion
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