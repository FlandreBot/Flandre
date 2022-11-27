using Flandre.Core.Messaging;

namespace Flandre.Core.Extensions;

/// <summary>
/// 上下文扩展方法
/// </summary>
public static class ContextExtensions
{
    /// <summary>
    /// 将 Bot 设置为群组主 Bot
    /// </summary>
    /// <param name="ctx">消息上下文</param>
    public static void SetBotAsGuildAssignee(this MessageContext ctx)
    {
        if (ctx.GuildId is null) return;
        ctx.App.SetGuildAssignee(ctx.Platform, ctx.GuildId, ctx.SelfId);
    }
}