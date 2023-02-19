using Flandre.Core.Messaging;
using Flandre.Framework.Attributes;
using Flandre.Framework.Common;
using Flandre.Framework.Extensions;

// ReSharper disable StringLiteralTypo

namespace Flandre.Framework.Tests;

public class TestPlugin : Plugin
{
    public override async Task OnMessageReceived(MessageContext ctx)
    {
        if (ctx.Message.GetText().StartsWith("OMR:"))
            await ctx.Bot.SendMessage(ctx.Message);
    }

    [Command("test1")]
    public static MessageContent OnTest1(CommandContext ctx, bool arg1, [Option] double opt = 0)
    {
        return $"{arg1} {opt + 200}";
    }

    [Command("test2")]
    public static MessageContent OnTest2(CommandContext ctx, int arg1, float arg2,
        [Option] bool opt1 = true, [Option(ShortName = 'o')] bool opt2 = false)
    {
        return $"{arg1} {arg2} {opt1} {opt2}";
    }

    [Command("sub.test")]
    [StringShortcut("子测试")]
    public static MessageContent? OnSubTest(CommandContext ctx)
    {
        return null;
    }

    [Command("...sub....sub..sub......test..")]
    public static MessageContent? OnSubSubSubTest(CommandContext ctx) => null;

    [Command("throw-ex")]
    public static MessageContent? OnThrowEx(CommandContext ctx) =>
        throw new Exception("Test Exception");

    [Command("start-session")]
    public static async Task<MessageContent?> OnStartSession(CommandContext ctx)
    {
        var nextMsg = await ctx.StartSession(TimeSpan.FromSeconds(2));
        return nextMsg?.Content;
    }
}