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

            var parser = ctx.CommandStringParser = new StringParser(commandStr);

            var root = parser.SkipWhiteSpaces().Read(' ');

            if (StringShortcuts.TryGetValue(root, out var command))
                return command;

            // TODO: support regex variables
            // ReSharper disable once AccessToModifiedClosure
            var matchedRegex = RegexShortcuts.Keys.FirstOrDefault(regex => regex.IsMatch(root));
            if (matchedRegex is not null)
                return RegexShortcuts[matchedRegex];

            if (!string.IsNullOrWhiteSpace(commandPrefix)
                && !root.StartsWith(commandPrefix))
                return null;

            var path = new List<string>();
            var current = root.TrimStart(commandPrefix);
            var node = RootCommandNode;
            var temp = false;

            while (!parser.SkipWhiteSpaces().IsEnd)
            {
                path.Add(current);
                if (node.Subcommands.TryGetValue(current, out var subNode))
                {
                    node = subNode;
                    if (temp) parser.Read(' ');
                    else temp = true;
                    current = parser.SkipWhiteSpaces().Peek(' ');
                }
                else
                {
                    break;
                }
            }

            if (node.HasCommand)
                return node.Command;

            if (!string.IsNullOrWhiteSpace(commandPrefix))
                ctx.Response = $"未找到指令：{string.Join('.', path)}。";
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

            if (!ctx.Command.TryParse(ctx.CommandStringParser, out var result))
                return result.ErrorText!;

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
            MessageContent? content = null;
            Exception? ex = null;
            try
            {
                content = ctx.Command.Invoke(plugin, cmdCtx, result);
            }
            catch (Exception e)
            {
                pluginLogger.LogError("Error occurred in ");
                ex = e;
            }

            OnCommandInvoked?.Invoke(this, new CommandInvokedEvent(ctx.Command, ctx.Message, ex, content));
            return content;
        }

        UseMiddleware((ctx, next) =>
        {
            ctx.Response = InvokeCommand(ctx) ?? ctx.Response;
            next();
        });
        return this;
    }
}