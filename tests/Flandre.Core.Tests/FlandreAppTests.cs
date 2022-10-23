using Flandre.Core.Attributes;
using Flandre.Core.Common;
using Flandre.Core.Messaging;
using Flandre.Adapters.Mock;

// ReSharper disable StringLiteralTypo

namespace Flandre.Core.Tests;

public class FlandreAppTests
{
    [Fact]
    public void TestFlandreApp()
    {
        var app = new FlandreApp();
        var adapter = new MockAdapter();

        var channelClient = adapter.GenerateChannelClient();
        var friendClient = adapter.GenerateFriendClient();

        app.OnAppReady += async (_, _) =>
        {
            var content = await channelClient.SendForReply("OMR:114514");
            Assert.Equal("OMR:114514", content?.GetText());

            content = await friendClient.SendForReply("test1 true --opt 114.514");
            Assert.Equal("arg1: True opt: 114.514 b: False t: True",
                content?.GetText());

            content = await friendClient.SendForReply("test1  -o 1919.810  false");
            Assert.Equal("arg1: False opt: 1919.81 b: False t: True",
                content?.GetText());

            content = await friendClient.SendForReply("test1 -bo 111.444 --no-trueopt false");
            Assert.Equal("arg1: False opt: 111.444 b: True t: False",
                content?.GetText());

            app.Stop();
        };

        app.Use(adapter).Use(new TestPlugin()).Start();
    }

    [Fact]
    public void TestCommandMap()
    {
        var app = new FlandreApp().Use(new TestPlugin());
        
        Assert.Equal(3, app.CommandMap.Count);
        Assert.NotNull(app.CommandMap.GetValueOrDefault("test1"));
        Assert.NotNull(app.CommandMap.GetValueOrDefault("sub.test"));
        Assert.NotNull(app.CommandMap.GetValueOrDefault("sub.sub.sub.test"));
    }
}

[Plugin("Test")]
public class TestPlugin : Plugin
{
    public override void OnMessageReceived(MessageContext ctx)
    {
        if (ctx.Message.GetText().StartsWith("OMR:"))
            ctx.Bot.SendMessage(ctx.Message);
    }

    [Command("test1 <arg1:bool>")]
    [Option("opt", "-o <opt:double>")]
    [Option("boolopt", "-b <:bool>")]
    [Option("trueopt", "-t <:bool=true>")]
    public static MessageContent OnTest1(MessageContext ctx, ParsedArgs args)
    {
        var arg1 = args.GetArgument<bool>("arg1");
        var opt = args.GetOption<double>("opt");
        var boolOpt = args.GetOption<bool>("boolopt");
        var trueOpt = args.GetOption<bool>("trueopt");
        return $"arg1: {arg1} opt: {opt} b: {boolOpt} t: {trueOpt}";
    }

    [Command("sub.test")]
    public static MessageContent? OnSubTest(MessageContext ctx, ParsedArgs args) => null;

    [Command("...sub....sub..sub......test..")]
    public static MessageContent? OnSubSubSubTest(MessageContext ctx, ParsedArgs args) => null;
}