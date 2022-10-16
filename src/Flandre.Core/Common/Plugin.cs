using System.Reflection;
using Flandre.Core.Attributes;
using Flandre.Core.Events.Bot;
using Flandre.Core.Events.Logger;
using Flandre.Core.Messaging;
using Flandre.Core.Utils;

namespace Flandre.Core.Common;

/// <summary>
/// 模块基类
/// </summary>
public abstract class Plugin : IModule
{
    /// <summary>
    /// 插件 Logger，默认使用插件名称
    /// </summary>
    public Logger Logger { get; }

    /// <summary>
    /// 插件的指令
    /// </summary>
    public List<Command> Commands { get; } = new();

    /// <summary>
    /// 插件信息
    /// </summary>
    public PluginAttribute PluginInfo { get; }

    /// <summary>
    /// 插件基类构造函数
    /// </summary>
    public Plugin()
    {
        var type = GetType();
        PluginInfo = type.GetCustomAttribute<PluginAttribute>() ?? new PluginAttribute(type.Name);
        Logger = new Logger(PluginInfo.Name);

        foreach (var method in type.GetMethods())
        {
            var attr = method.GetCustomAttribute<CommandAttribute>();
            if (attr is null) continue;

            var options = method.GetCustomAttributes<OptionAttribute>().ToList();

            Commands.Add(new Command(this, attr, method, options));
        }
    }

    internal MessageContent GetHelp()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 处理消息事件
    /// </summary>
    /// <param name="ctx">消息上下文</param>
    public virtual void OnMessageReceived(MessageContext ctx)
    {
    }

    /// <summary>
    /// 收到拉群邀请
    /// </summary>
    /// <param name="ctx">当前上下文</param>
    /// <param name="e">拉群邀请事件</param>
    public virtual void OnGuildInvited(Context ctx, BotGuildInvitedEvent e)
    {
    }

    /// <summary>
    /// 收到入群申请
    /// </summary>
    /// <param name="ctx">当前上下文</param>
    /// <param name="e">入群申请事件</param>
    public virtual void OnGuildRequested(Context ctx, BotGuildRequestedEvent e)
    {
    }

    /// <summary>
    /// 收到好友申请
    /// </summary>
    /// <param name="ctx">当前上下文</param>
    /// <param name="e">好友申请事件</param>
    public virtual void OnFriendRequested(Context ctx, BotFriendRequestedEvent e)
    {
    }

    /// <summary>
    /// 日志记录前触发的事件
    /// </summary>
    /// <param name="ctx">当前上下文</param>
    /// <param name="e">日志记录前事件</param>
    public virtual void OnLoggerLogging(Context ctx, LoggerLoggingEvent e)
    {
    }
}