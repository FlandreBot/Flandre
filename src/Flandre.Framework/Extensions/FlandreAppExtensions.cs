using Flandre.Core.Common;

// ReSharper disable once CheckNamespace
namespace Flandre.Framework;

/// <summary>
/// <see cref="FlandreApp"/> 的扩展方法
/// </summary>
public static class FlandreAppExtensions
{
    /// <summary>
    /// 运行应用实例，并自动注册内置中间件
    /// </summary>
    public static Task StartWithDefaultsAsync(this FlandreApp app, CancellationToken cancellationToken = default)
    {
        app.UseCommandSession();
        app.UseCommandParser();
        app.UseCommandInvoker();
        return app.StartAsync(cancellationToken);
    }

    /// <summary>
    /// 设置群组代理 <see cref="Bot"/>（主 bot）
    /// </summary>
    /// <param name="app"><see cref="FlandreApp"/> 实例</param>
    /// <param name="platform">平台</param>
    /// <param name="guildId">群组 ID</param>
    /// <param name="botId"><see cref="Bot.SelfId"/></param>
    public static void SetGuildAssignee(this FlandreApp app, string platform, string guildId, string botId)
    {
        app.GuildAssignees.AddOrUpdate($"{platform}:{guildId}", botId, (_, _) => botId);
    }

    /// <summary>
    /// 检查群组是否已被代理（已设置主 bot）
    /// </summary>
    /// <param name="app"><see cref="FlandreApp"/> 实例</param>
    /// <param name="platform">平台</param>
    /// <param name="botId"><see cref="Bot.SelfId"/></param>
    public static bool IsGuildAssigned(this FlandreApp app, string platform, string botId)
    {
        return app.GuildAssignees.ContainsKey($"{platform}:{botId}");
    }
}
