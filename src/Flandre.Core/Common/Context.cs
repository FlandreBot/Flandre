namespace Flandre.Core.Common;

/// <summary>
/// 基础上下文
/// </summary>
public class Context
{
    /// <summary>
    /// 当前 bot 实例
    /// </summary>
    public Bot Bot { get; }

    /// <summary>
    /// 构造上下文
    /// </summary>
    /// <param name="bot">bot 实例</param>
    public Context(Bot bot)
    {
        Bot = bot;
    }

    /// <summary>
    /// Bot 所在平台，等同于 Bot.Platform。
    /// </summary>
    public string Platform => Bot.Platform;

    /// <summary>
    /// Bot 自身 ID，等同于 Bot.SelfId。
    /// </summary>
    public string SelfId => Bot.SelfId;
}
