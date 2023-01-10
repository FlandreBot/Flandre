using Flandre.Core.Messaging;
using Flandre.Core.Messaging.Segments;
using Flandre.Core.Utils;
using Flandre.Framework.Common;
using Flandre.Framework.Events;
using Flandre.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Flandre.Framework;

public sealed partial class FlandreApp
{
    private void CheckGuildAssigneeMiddleware(MiddlewareContext ctx, Action next)
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

    private async Task PluginMessageEventMiddleware(MiddlewareContext ctx, Action next)
    {
        await Task.WhenAll(_pluginTypes
            .Select(p => ((Plugin)Services.GetRequiredService(p)).OnMessageReceived(ctx))
            .ToArray());
        next();
    }

    private void CheckCommandSessionMiddleware(MiddlewareContext ctx, Action next)
    {
        var mark = ctx.GetUserMark();
        if (CommandSessions.TryGetValue(mark, out var tcs))
        {
            CommandSessions.TryRemove(mark, out _);
            tcs.TrySetResult(ctx.Message);
        }
        else
        {
            next();
        }
    }

    private void ParseCommandMiddleware(MiddlewareContext ctx, Action next)
    {
        ctx.Response = ParseCommand(ctx);
        next();
    }

    private MessageContent? ParseCommand(MiddlewareContext ctx)
    {
        MessageContent? ParseAndInvoke(Command cmd, StringParser p)
        {
            var (args, error) = cmd.ParseCommand(p);
            if (error is not null) return error;

            var plugin = (Plugin)ctx.Services.GetRequiredService(cmd.PluginType);
            var pluginLogger = (ILogger)Services.GetRequiredService(plugin.LoggerType);

            var invocationCancelled = false;
            if (OnCommandInvoking is not null)
            {
                var invokingEvent = new CommandInvokingEvent(cmd, ctx.Message);
                OnCommandInvoking.Invoke(this, invokingEvent);
                invocationCancelled = invokingEvent.IsCancelled;
            }

            if (invocationCancelled)
                return null;

            var cmdCtx = new CommandContext(ctx.App, ctx.Bot, ctx.Message);
            var (content, ex) = cmd.InvokeCommand(plugin, cmdCtx, args, pluginLogger);
            OnCommandInvoked?.Invoke(this, new CommandInvokedEvent(cmd, ctx.Message, ex, content));
            return content;
        }

        var commandStr = ctx.Message.GetText().Trim();
        var commandPrefix = _appOptions.CurrentValue.CommandPrefix;
        if (commandStr == commandPrefix)
            return null;

        var parser = new StringParser(commandStr);

        var root = parser.SkipSpaces().Read(' ');

        if (ShortcutMap.TryGetValue(root, out var command))
            return ParseAndInvoke(command, parser);

        if (!string.IsNullOrWhiteSpace(commandPrefix)
            && !root.StartsWith(commandPrefix))
            return null;
        root = root.TrimStart(commandPrefix);
        parser.SkipSpaces();

        var notFound = root;

        for (var count = 0;; count++)
        {
            if (CommandMap.TryGetValue(root, out command) &&
                (parser.IsEnd() || !CommandMap.Keys.Any(cmd =>
                    cmd.StartsWith($"{root}.{parser.Peek(' ')}"))))
                return ParseAndInvoke(command, parser);

            if (parser.SkipSpaces().IsEnd()) break;
            root = $"{root}.{parser.Read(' ')}";

            if (count < 4) notFound = root;
        }

        if (string.IsNullOrWhiteSpace(commandPrefix))
            return null;

        return $"未找到指令：{notFound}。";
    }
}