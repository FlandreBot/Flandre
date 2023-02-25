using Flandre.Core.Events;
using Flandre.Framework.Events;

namespace Flandre.Framework;

/// <summary>
/// 应用事件处理
/// </summary>
/// <typeparam name="TEvent"></typeparam>
/// <param name="app"></param>
/// <param name="e"></param>
public delegate void AppEventHandler<in TEvent>(FlandreApp app, TEvent e) where TEvent : BaseEvent;

public sealed partial class FlandreApp
{
    /// <summary>
    /// 应用正在启动
    /// </summary>
    public event AppEventHandler<AppStartingEvent>? OnStarting;

    /// <summary>
    /// 应用就绪
    /// </summary>
    public event AppEventHandler<AppReadyEvent>? OnReady;

    /// <summary>
    /// 应用已经退出
    /// </summary>
    public event AppEventHandler<AppStoppedEvent>? OnStopped;

    /// <summary>
    /// 应用正在触发指令
    /// </summary>
    public event AppEventHandler<CommandInvokingEvent>? OnCommandInvoking;

    /// <summary>
    /// 应用触发了指令
    /// </summary>
    public event AppEventHandler<CommandInvokedEvent>? OnCommandInvoked;
}
