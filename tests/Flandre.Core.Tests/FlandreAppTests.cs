using Flandre.Core.Attributes;
using Flandre.Core.Common;
using Flandre.Core.Messaging;
using Flandre.Adapters.Mock;
using Flandre.Core.Utils;

// ReSharper disable StringLiteralTypo

namespace Flandre.Core.Tests;

public class FlandreAppTests
{
    [Fact]
    public void TestFlandreApp()
    {
        Logger.ThrowOnError = true;
        var app = new FlandreApp();
        var adapter = new MockAdapter();

        var channelClient = adapter.GetChannelClient();
        var friendClient = adapter.GetFriendClient();

        app.OnAppReady += (_, _) =>
        {
            var content = channelClient.SendForReply("OMR:114514").Result;
            Assert.Equal("OMR:114514", content?.GetText());
            // throw new Exception();

            content = friendClient.SendForReply("test1 true --opt 114.514").Result;
            Assert.Equal("arg1: True opt: 114.514 b: False t: True",
                content?.GetText());

            content = friendClient.SendForReply("test1  -o 1919.810  false").Result;
            Assert.Equal("arg1: False opt: 1919.81 b: False t: True",
                content?.GetText());

            content = friendClient.SendForReply("test1 -bo 111.444 --no-trueopt false").Result;
            Assert.Equal("arg1: False opt: 111.444 b: True t: False",
                content?.GetText());

            app.Stop();
        };

        app.Use(adapter).Use(new TestPlugin()).Start();
    }

    [Fact]
    public void TestShortcutAndAlias()
    {
        Logger.ThrowOnError = true;
        var app = new FlandreApp();
        var adapter = new MockAdapter();

        var channelClient = adapter.GetChannelClient();

        app.OnAppReady += (_, _) =>
        {
            Assert.Equal(5, app.CommandMap.Count);
            Assert.Equal(2, app.ShortcutMap.Count);
            
            Assert.NotNull(app.CommandMap.GetValueOrDefault("test1"));
            Assert.NotNull(app.CommandMap.GetValueOrDefault("sub.test"));
            Assert.NotNull(app.CommandMap.GetValueOrDefault("sub.sub.sub.test"));
            
            // shortcut
            Assert.NotNull(app.ShortcutMap.GetValueOrDefault("测试"));
            Assert.NotNull(app.ShortcutMap.GetValueOrDefault("子测试"));
            Assert.Equal(app.ShortcutMap["测试"], app.CommandMap["test1"]);
            
            // alias
            Assert.NotNull(app.CommandMap.GetValueOrDefault("test111.11.45.14"));
            Assert.Equal(app.CommandMap["sssuuubbb"], app.CommandMap["sub.test"]);

            var result = "arg1: True opt: 114.514 b: False t: True";

            var content = channelClient.SendForReply("test1 true --opt 114.514").Result;
            Assert.Equal(result, content?.GetText());

            content = channelClient.SendForReply(" 测试  true --opt 114.514").Result;
            Assert.Equal(result, content?.GetText());

            app.Stop();
        };

        app.Use(adapter).Use(new TestPlugin()).Start();
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
    [Shortcut("测试")]
    [Alias("..test111.11..45...14...")] // test normalize
    public static MessageContent OnTest1(MessageContext ctx, ParsedArgs args)
    {
        var arg1 = args.GetArgument<bool>("arg1");
        var opt = args.GetOption<double>("opt");
        var boolOpt = args.GetOption<bool>("boolopt");
        var trueOpt = args.GetOption<bool>("trueopt");
        return $"arg1: {arg1} opt: {opt} b: {boolOpt} t: {trueOpt}";
    }

    [Command("sub.test")]
    [Alias("sssuuubbb")]
    [Shortcut("子测试")]
    public static MessageContent? OnSubTest(MessageContext ctx, ParsedArgs args) => null;

    [Command("...sub....sub..sub......test..")]
    public static MessageContent? OnSubSubSubTest(MessageContext ctx, ParsedArgs args) => null;
}