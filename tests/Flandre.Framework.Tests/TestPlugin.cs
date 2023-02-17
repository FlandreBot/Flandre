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

    protected internal override Task OnLoading(PluginLoadContext ctx)
    {
        ctx.AddCommand("test")
            .WithAction<string>(OnTest);

        // 根据CmdAttribute添加Command
        ctx.AddCommandFromAttributes();

        return Task.CompletedTask;
    }

    [Command("test")]
    public async Task OnTest(CommandContext ctx, string str)
    {
        await ctx.Bot.SendMessage(ctx.Message, str);
    }

    [Shortcut("测试")]
    [Alias("..test111.11..45...14...")] // test normalize


    [Cmd(nameof(OnTest1), "Alias1", "Alias2", Father = nameof(OnTest))]
    public static MessageContent OnTest1(
        CommandContext ctx,
        int arg1,
        double arg2,
        [Option] object arg3,
        [Option] double arg4
        )
    {

    }

    [Command("test2 <arg1:text>")]
    public static MessageContent OnTest2(CommandContext _, ParsedArgs args)
    {
        var arg1 = args.GetArgument<string>("arg1");
        return arg1;
    }

    [Command("sub.test")]
    [Alias("sssuuubbb")]
    [Shortcut("子测试")]
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
