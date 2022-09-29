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
            
            content = await friendClient.SendForReply("test1 true");
            Assert.Equal("arg1: True", content?.GetText());

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
    public static MessageContent OnTest1(MessageContext ctx, ParsedArgs args)
    {
        var arg1 = args.GetArgument<bool>("arg1");
        return $"arg1: {arg1}";
    }
}