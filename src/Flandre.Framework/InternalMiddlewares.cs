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
    private bool _assigneeCheckerUsed;
    private bool _pluginMessageEventUsed;
    private bool _commandSessionUsed;
    private bool _commandParserUsed;
    private bool _commandInvokerUsed;

    public FlandreApp UseAssigneeCheckerMiddleware()
    {
        if (_assigneeCheckerUsed) return this;
        _assigneeCheckerUsed = true;
        UseMiddleware((ctx, next) =>
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
        });
        return this;
    }

    public FlandreApp UsePluginMessageEventMiddleware()
    {
        if (_pluginMessageEventUsed) return this;
        _pluginMessageEventUsed = true;
        UseMiddleware(async (ctx, next) =>
        {
            await Task.WhenAll(_pluginTypes
                .Select(p => ((Plugin)Services.GetRequiredService(p)).OnMessageReceived(ctx))
                .ToArray());
            next();
        });
        return this;
    }

    public FlandreApp UseCommandSessionMiddleware()
    {
        if (_commandSessionUsed) return this;
        _commandSessionUsed = true;
        UseMiddleware((ctx, next) =>
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
        });
        return this;
    }

    public FlandreApp UseCommandParserMiddleware()
    {
        if (_commandParserUsed) return this;
        _commandParserUsed = true;

        Command? ParseCommand(MiddlewareContext ctx)
        {
            var commandStr = ctx.Message.GetText().Trim();
            var commandPrefix = _appOptions.CurrentValue.CommandPrefix;
            if (commandStr == commandPrefix)
                return null;

            ctx.CommandStringParser = new StringParser(commandStr);

            var root = ctx.CommandStringParser.SkipSpaces().Read(' ');

            if (ShortcutMap.TryGetValue(root, out var command))
                return command;

            if (!string.IsNullOrWhiteSpace(commandPrefix)
                && !root.StartsWith(commandPrefix))
                return null;

            root = root.TrimStart(commandPrefix);
            ctx.CommandStringParser.SkipSpaces();

            var notFound = root;

            for (var count = 0;; count++)
            {
                var next = $"{root}.{ctx.CommandStringParser.Peek(' ')}";
                // ReSharper disable once AccessToModifiedClosure
                if (CommandMap.TryGetValue(root, out command) &&
                    (ctx.CommandStringParser.IsEnd() || !CommandMap.Keys.Any(cmd =>
                        cmd.StartsWith(next))))
                {
                    return command;
                }

                if (ctx.CommandStringParser.SkipSpaces().IsEnd()) break;
                root = $"{root}.{ctx.CommandStringParser.Read(' ')}";

                if (count < 4) notFound = root;
            }

            if (!string.IsNullOrWhiteSpace(commandPrefix))
                ctx.Response = $"未找到指令：{notFound}。";
            return null;
        }

        UseMiddleware((ctx, next) =>
        {
            ctx.Command = ParseCommand(ctx);
            next();
        });
        return this;
    }

    public FlandreApp UseCommandInvokerMiddleware()
    {
        if (_commandInvokerUsed) return this;
        _commandInvokerUsed = true;

        MessageContent? InvokeCommand(MiddlewareContext ctx)
        {
            if (ctx.Command is null || ctx.CommandStringParser is null)
                return null;

            var (args, error) = ctx.Command.ParseCommand(ctx.CommandStringParser);
            if (error is not null) return error;

            var plugin = (Plugin)ctx.Services.GetRequiredService(ctx.Command.PluginType);
            var pluginLogger = (ILogger)Services.GetRequiredService(plugin.LoggerType);

            var invocationCancelled = false;
            if (OnCommandInvoking is not null)
            {
                var invokingEvent = new CommandInvokingEvent(ctx.Command, ctx.Message);
                OnCommandInvoking.Invoke(this, invokingEvent);
                invocationCancelled = invokingEvent.IsCancelled;
            }

            if (invocationCancelled)
                return null;

            var cmdCtx = new CommandContext(ctx.App, ctx.Bot, ctx.Message);
            var (content, ex) = ctx.Command.InvokeCommand(plugin, cmdCtx, args, pluginLogger);
            OnCommandInvoked?.Invoke(this, new CommandInvokedEvent(ctx.Command, ctx.Message, ex, content));
            return content;
        }

        UseMiddleware((ctx, next) =>
        {
            ctx.Response = InvokeCommand(ctx);
            next();
        });
        return this;
    }
}