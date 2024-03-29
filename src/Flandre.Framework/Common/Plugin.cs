using Flandre.Core.Common;
using Flandre.Core.Events;
using Flandre.Core.Messaging;
using Flandre.Framework.Routing;
using Microsoft.Extensions.Logging;

namespace Flandre.Framework.Common;

/// <summary>
/// 插件
/// </summary>
public abstract class Plugin
{
    /// <summary>
    /// 缓存日志类型
    /// </summary>
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
    /// 映射指令时调用
    /// </summary>
    /// <returns></returns>
    public virtual void OnCommandMapping(ICommandRouteBuilder builder) { }

    /// <summary>
    /// 加载插件时调用
    /// </summary>
    /// <returns></returns>
    public virtual Task OnLoadingAsync() => Task.CompletedTask;

    /// <summary>
    /// 卸载插件时调用
    /// </summary>
    /// <returns></returns>
    public virtual Task OnUnloadingAsync() => Task.CompletedTask;

    /// <summary>
    /// 处理消息事件
    /// </summary>
    /// <param name="ctx">当前消息上下文</param>
    public virtual Task OnMessageReceivedAsync(MessageContext ctx) => Task.CompletedTask;

    /// <summary>
    /// 收到拉群邀请
    /// </summary>
    /// <param name="ctx">当前上下文</param>
    /// <param name="e">拉群邀请事件</param>
    public virtual Task OnGuildInvitedAsync(BotContext ctx, BotGuildInvitedEvent e) => Task.CompletedTask;

    /// <summary>
    /// 收到入群申请
    /// </summary>
    /// <param name="ctx">当前上下文</param>
    /// <param name="e">入群申请事件</param>
    public virtual Task OnGuildJoinRequestedAsync(BotContext ctx, BotGuildJoinRequestedEvent e) => Task.CompletedTask;

    /// <summary>
    /// 收到好友申请
    /// </summary>
    /// <param name="ctx">当前上下文</param>
    /// <param name="e">好友申请事件</param>
    public virtual Task OnFriendRequestedAsync(BotContext ctx, BotFriendRequestedEvent e) => Task.CompletedTask;
}
