namespace Flandre.Core.Common;

/// <summary>
/// 基础上下文
/// </summary>
public class Context
{
    /// <summary>
    /// FlandreApp 实例
    /// </summary>
    public FlandreApp App { get; }

    /// <summary>
    /// 当前 bot 实例
    /// </summary>
    public Bot Bot { get; }

    /// <summary>
    /// 构造上下文
    /// </summary>
    /// <param name="app">FlandreApp 实例</param>
    /// <param name="bot">bot 实例</param>
    public Context(FlandreApp app, Bot bot)
    {
        App = app;
        Bot = bot;
    }

    /// <summary>
    /// Bot 所在平台，等同于 Bot.Platform。
    /// </summary>
    public string Platform => Bot.Platform;
}