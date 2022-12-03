using Flandre.Core.Messaging;
using Flandre.Core.Messaging.Segments;
using Flandre.Core.Utils;
using Flandre.Framework.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Flandre.Framework;

public sealed partial class FlandreApp
{
    internal void CheckGuildAssigneeMiddleware(MiddlewareContext ctx, Action next)
    {
        var segment = ctx.Message.Content.Segments.FirstOrDefault();
        if (segment is AtSegment ats)
        {
            if (ats.UserId == ctx.SelfId)
                next();
        }
        // 如果没找到群组的 assignee
        else if (!GuildAssignees.TryGetValue($"{ctx.Platform}:{ctx.GuildId}", out var assignee))
        {
            next();
        }
        // 如果找到了群组的 assignee，且是自己
        else if (ctx.SelfId == assignee)
        {
            next();
        }
    }

    internal async Task PluginMessageEventMiddleware(MiddlewareContext ctx, Action next)
    {
        await Task.WhenAll(_pluginTypes
            .Select(p => ((Plugin)Services.GetRequiredService(p)).OnMessageReceived(ctx))
            .ToArray());
        next();
    }

    internal void ParseCommandMiddleware(MiddlewareContext ctx, Action next)
    {
        var cmdCtx = new CommandContext(ctx.App, ctx.Bot, ctx.Message);
        ctx.Response = ParseCommand(cmdCtx);
        next();
    }

    internal MessageContent? ParseCommand(CommandContext ctx)
    {
        MessageContent? ParseAndInvoke(Command cmd, StringParser p)
        {
            var (args, error) = cmd.ParseCommand(p);
            if (error is not null) return error;

            var plugin = (Plugin)Services.GetRequiredService(cmd.PluginType);
            var pluginLogger = Services.GetRequiredService<ILoggerFactory>().CreateLogger(cmd.PluginType);

            return cmd.InvokeCommand(plugin, ctx, args, pluginLogger);
        }

        var commandStr = ctx.Message.GetText().Trim();
        if (commandStr == _config.CommandPrefix)
            return null;

        var parser = new StringParser(commandStr);

        var root = parser.SkipSpaces().Read(' ');

        if (ShortcutMap.TryGetValue(root, out var command))
            return ParseAndInvoke(command, parser);

        if (!string.IsNullOrWhiteSpace(_config.CommandPrefix)
            && !root.StartsWith(_config.CommandPrefix))
            return null;
        root = root.TrimStart(_config.CommandPrefix);
        parser.SkipSpaces();

        while (true)
        {
            if (CommandMap.TryGetValue(root, out command) &&
                (parser.IsEnd() || !CommandMap.Keys.Any(cmd =>
                    cmd.StartsWith($"{root}.{parser.Peek(' ')}"))))
                return ParseAndInvoke(command, parser);

            if (parser.SkipSpaces().IsEnd()) break;
            root = $"{root}.{parser.Read(' ')}";
        }

        if (string.IsNullOrWhiteSpace(_config.CommandPrefix)) return null;
        ctx.Bot.SendMessage(ctx.Message, $"未找到指令：{root}。").Wait();

        return null;
    }
}