using Flandre.Core.Messaging;
using Flandre.Core.Messaging.Segments;
using Flandre.Core.Utils;
using Flandre.Framework.Common;
using Flandre.Framework.Events;
using Flandre.Framework.Services;
using Flandre.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Flandre.Framework;

public sealed partial class FlandreApp
{
    private bool CheckMiddlewareUsed(string methodName)
    {
        var symbol = $"__Flandre_{methodName[3..]}_Middleware_Used";
        if (Properties.ContainsKey(symbol))
            return true;

        // not used
        Properties[symbol] = true;
        return false;
    }

    private FlandreApp UsePluginMessageHandler()
    {
        if (CheckMiddlewareUsed(nameof(UsePluginMessageHandler)))
            return this;

        UseMiddleware(async (ctx, next) =>
        {
            _pluginTypes.ForEach(p => ((Plugin)Services.GetRequiredService(p)).OnMessageReceivedAsync(ctx));
            await next();
        });
        return this;
    }

    /// <summary>
    /// 使用群组 assignee 检查中间件
    /// </summary>
    public FlandreApp UseAssigneeChecker()
    {
        if (CheckMiddlewareUsed(nameof(UseAssigneeChecker)))
            return this;

        UseMiddleware(async (ctx, next) =>
        {
            var segment = ctx.Message.Content.Segments.FirstOrDefault();
            if (segment is AtSegment ats)
            {
                if (ats.UserId == ctx.SelfId)
                    await next();
            }
            // 如果没找到群组的 assignee
            else if (!GuildAssignees.TryGetValue($"{ctx.Platform}:{ctx.GuildId}", out var assignee))
            {
                await next();
            }
            // 如果找到了群组的 assignee，且是自己
            else if (ctx.SelfId == assignee)
            {
                await next();
            }
        });
        return this;
    }

    /// <summary>
    /// 添加指令会话中间层
    /// </summary>
    /// <remarks>用于指令等待下一条语句</remarks>
    /// <returns></returns>
    public FlandreApp UseCommandSession()
    {
        if (CheckMiddlewareUsed(nameof(UseCommandSession)))
            return this;

        UseMiddleware(async (ctx, next) =>
        {
            var mark = ctx.GetUserMark();
            if (CommandSessions.TryGetValue(mark, out var tcs))
            {
                CommandSessions.TryRemove(mark, out _);
                tcs.TrySetResult(ctx.Message);
            }
            else
            {
                await next();
            }
        });
        return this;
    }

    /// <summary>
    /// 添加指令解析中间层
    /// </summary>
    /// <remarks>解析 <see cref="MessageContext.Message"/> 并得到 <see cref="MiddlewareContext.Command"/></remarks>
    /// <returns></returns>
    public FlandreApp UseCommandParser()
    {
        if (CheckMiddlewareUsed(nameof(UseCommandParser)))
            return this;

        UseMiddleware(async (ctx, next) =>
        {
            ctx.Command = ParseCommand(ctx);
            await next();
        });

        return this;

        Command? ParseCommand(MiddlewareContext ctx)
        {
            // 1. 检查 StringShortcut 中是否有匹配项
            // 3. 检查 RegexShortcut 中是否有匹配项
            // 4. 常规匹配指令

            var cmdService = Services.GetRequiredService<CommandService>();

            var commandStr = ctx.Message.GetText().Trim();

            foreach (var strShortcut in cmdService.StringShortcuts.Keys)
            {
                if (!strShortcut.TryFormat(commandStr, out var result))
                    continue;
                ctx.CommandStringParser = new StringParser(result);
                return cmdService.StringShortcuts[strShortcut];
            }

            foreach (var regShortcut in cmdService.RegexShortcuts.Keys)
            {
                if (!regShortcut.TryFormat(commandStr, out var result))
                    continue;
                ctx.CommandStringParser = new StringParser(result);
                return cmdService.RegexShortcuts[regShortcut];
            }

            var commandPrefix = _appOptions.CurrentValue.CommandPrefix;
            if (commandStr == commandPrefix)
                return null;

            var parser = ctx.CommandStringParser = new StringParser(commandStr);

            var root = parser.SkipWhiteSpaces().Read(' ');

            if (!string.IsNullOrWhiteSpace(commandPrefix)
                && !root.StartsWith(commandPrefix))
                return null;

            var path = new List<string>();
            var current = root.TrimStart(commandPrefix);
            var node = cmdService.RootCommandNode;
            var temp = false;
            parser.SkipWhiteSpaces();

            do
            {
                var cur = current;
                path.Add(cur);
                if (node.SubNodes.Keys.FirstOrDefault(k =>
                        cur.Equals(k, StringComparison.OrdinalIgnoreCase)) is { } key)
                {
                    node = node.SubNodes[key];
                    if (temp)
                        parser.Read(' ');
                    else
                        temp = true;
                    current = parser.SkipWhiteSpaces().Peek(' ');
                }
                else
                {
                    break;
                }
            } while (!parser.SkipWhiteSpaces().IsEnd);

            if (node.HasCommand)
                return node.Command;

            if (!string.IsNullOrWhiteSpace(commandPrefix) &&
                !_appOptions.CurrentValue.IgnoreWhenCallingUndefinedCommand)
                ctx.Response = $"未找到指令：{string.Join('.', path)}。";
            return null;
        }
    }

    /// <summary>
    /// 添加指令触发中间层
    /// </summary>
    /// <remarks>触发 <see cref="MiddlewareContext.Command"/> 并得到 <see cref="MiddlewareContext.Response"/></remarks>
    /// <returns></returns>
    public FlandreApp UseCommandInvoker()
    {
        if (CheckMiddlewareUsed(nameof(UseCommandInvoker)))
            return this;

        UseMiddleware(async (ctx, next) =>
        {
            ctx.Response = await InvokeCommand(ctx) ?? ctx.Response;
            await next();
        });

        return this;

        async Task<MessageContent?> InvokeCommand(MiddlewareContext ctx)
        {
            if (ctx.Command is null || ctx.CommandStringParser is null)
                return null;

            var cmdService = Services.GetRequiredService<CommandService>();

            if (!ctx.Command.TryParse(ctx.CommandStringParser, cmdService, out var result))
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
                content = await ctx.Command.InvokeAsync(plugin, cmdCtx, result, pluginLogger);
            }
            catch (Exception e)
            {
                ex = e.InnerException ?? e;
                pluginLogger.LogError(ex, "Error occurred in {CommandPath}", ctx.Command.FullName);
            }

            OnCommandInvoked?.Invoke(this, new CommandInvokedEvent(ctx.Command, ctx.Message, ex, content));
            return content;
        }
    }
}
