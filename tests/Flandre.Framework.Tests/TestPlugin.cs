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
    }

    [Command("test")]
    public async Task OnTest(CommandContext ctx, string str)
    {
        await ctx.Bot.SendMessage(ctx.Message, str);
    }

    [Command("test1 <arg1:bool>")]
    [Option("opt")]
    [Option("boolopt")]
    [Option("trueopt")]
    [Shortcut("测试")]
    [Alias("..test111.11..45...14...")] // test normalize
    public static MessageContent OnTest1(CommandContext ctx)
    {
        var arg1 = args.GetArgument<bool>("arg1");
        var opt = args.GetOption<double>("opt");
        var boolOpt = args.GetOption<bool>("boolopt");
        var trueOpt = args.GetOption<bool>("trueopt");
        return $"arg1: {arg1} opt: {opt} b: {boolOpt} t: {trueOpt}";
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
    public static MessageContent? OnSubTest(CommandContext ctx) => null;

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