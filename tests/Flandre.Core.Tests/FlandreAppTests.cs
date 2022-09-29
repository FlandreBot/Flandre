using Flandre.Core.Attributes;
using Flandre.Core.Common;
using Flandre.Core.Messaging;
using Flandre.TestKit;

namespace Flandre.Core.Tests;

public class FlandreAppTests
{
    [Fact]
    public void TestFlandreApp()
    {
        var app = new FlandreApp();
        var adapter = new TestAdapter();

        var channelClient = adapter.GenerateChannelClient();
        var friendClient = adapter.GenerateFriendClient();

        app.OnAppReady += async (_, _) =>
        {
            var content = await channelClient.SendForReply("114514");
            Assert.Equal("114514", content?.GetText());
            
            content = await friendClient.SendForReply("test1 true --opt 114.514");
            Assert.Equal("arg1: True opt: 114.514", content?.GetText());
            
            content = await friendClient.SendForReply("test1  -o 1919.810  false");
            Assert.Equal("arg1: False opt: 1919.81", content?.GetText());

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
        ctx.Bot.SendMessage(ctx.Message);
    }

    [Command("test1 <arg1:bool>")]
    [Option("opt", "-o <opt:double>")]
    public static MessageContent OnTest1(MessageContext ctx, ParsedArgs args)
    {
        var arg1 = args.GetArgument<bool>("arg1");
        var opt = args.Options.GetOrDefault<double>("opt");
        return $"arg1: {arg1} opt: {opt}";
    }
}