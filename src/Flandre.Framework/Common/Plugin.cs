using Flandre.Core.Common;
using Flandre.Core.Events;
using Flandre.Core.Messaging;
using Microsoft.Extensions.Logging;

namespace Flandre.Framework.Common;

public abstract class Plugin
{
    // 缓存日志类型
    private static Type? _loggerType;

    internal Type LoggerType
    {
        get
        {
            _loggerType ??= typeof(ILogger<>).MakeGenericType(GetType());
            return _loggerType;
        }
    }

    /// <summary>
    /// 加载插件时调用
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    public virtual Task OnLoading(PluginLoadContext ctx) => Task.CompletedTask;

    /// <summary>
    /// 处理消息事件
    /// </summary>
    /// <param name="ctx">当前消息上下文</param>
    public virtual Task OnMessageReceived(MessageContext ctx) => Task.CompletedTask;

    /// <summary>
    /// 收到拉群邀请
    /// </summary>
    /// <param name="ctx">当前上下文</param>
    /// <param name="e">拉群邀请事件</param>
    public virtual Task OnGuildInvited(Context ctx, BotGuildInvitedEvent e) => Task.CompletedTask;

    /// <summary>
    /// 收到入群申请
    /// </summary>
    /// <param name="ctx">当前上下文</param>
    /// <param name="e">入群申请事件</param>
    public virtual Task OnGuildJoinRequested(Context ctx, BotGuildJoinRequestedEvent e) => Task.CompletedTask;

    /// <summary>
    /// 收到好友申请
    /// </summary>
    /// <param name="ctx">当前上下文</param>
    /// <param name="e">好友申请事件</param>
    public virtual Task OnFriendRequested(Context ctx, BotFriendRequestedEvent e) => Task.CompletedTask;
}