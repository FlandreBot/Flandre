﻿namespace Flandre.Core.Extensions;

/// <summary>
/// <see cref="FlandreApp" /> 的扩展方法
/// </summary>
public static class AppExtensions
{
    /// <summary>
    /// 设置群组代理 Bot（主 Bot）
    /// </summary>
    /// <param name="app">FlandreApp 实例</param>
    /// <param name="platform">平台</param>
    /// <param name="guildId">群组 ID</param>
    /// <param name="botId">Bot.SelfId</param>
    public static void SetGuildAssignee(this FlandreApp app, string platform, string guildId, string botId)
    {
        app.GuildAssignees.TryAdd($"{platform}:{guildId}", botId);
    }

    /// <summary>
    /// 检查群组是否已被代理（已设置主 bot）
    /// </summary>
    /// <param name="app">FlandreApp 实例</param>
    /// <param name="platform">平台</param>
    /// <param name="botId">Bot.SelfId</param>
    public static bool IsGuildAssigned(this FlandreApp app, string platform, string botId)
    {
        return app.GuildAssignees.ContainsKey($"{platform}:{botId}");
    }
}