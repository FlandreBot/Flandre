using Flandre.Core.Messaging;
using Flandre.Core.Messaging.Segments;

namespace Flandre.Core;

public partial class FlandreApp
{
    internal void CheckAssigneeMiddleware(MessageContext ctx, Action next)
    {
        var segment = ctx.Message.Content.GetSegment<AtSegment>();
        if (ctx.SelfId == segment?.UserId)
            next();
        // 如果没找到群组的 assignee
        else if (!GuildAssignees.TryGetValue($"{ctx.Platform}:{ctx.GuildId}", out var assignee))
            next();
        // 如果找到了群组的 assignee，且是自己
        else if (ctx.SelfId == assignee)
            next();
    }
    
    internal void PluginMessageReceivedMiddleware(MessageContext ctx, Action next)
    {
        foreach (var plugin in Plugins)
            plugin.OnMessageReceived(ctx);
        next();
    }
    
    internal void ParseCommandMiddleware(MessageContext ctx, Action next)
    {
        var content = ParseCommand(ctx);
        if (content is not null)
            ctx.Bot.SendMessage(ctx.Message, content);
        next();
    }
}