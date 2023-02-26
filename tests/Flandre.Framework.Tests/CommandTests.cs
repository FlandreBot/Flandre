using System.ComponentModel;

namespace Flandre.Framework.Tests;

public class CommandTests
{
    public class TestPlugin : Plugin
    {
        public override async Task OnMessageReceived(MessageContext ctx)
        {
            if (ctx.Message.GetText().StartsWith("OMR:"))
                await ctx.Bot.SendMessage(ctx.Message);
        }

        [Command("test1")]
        [Description("This is a test command.")]
        public static MessageContent OnTest1(CommandContext ctx, bool arg1, [Option] double opt = 0)
        {
            return $"{arg1} {opt + 200}";
        }

        [Command("test2", "..test111.11...45.14.")]
        [Obsolete("This command is obsoleted.")]
        public static MessageContent OnTest2(CommandContext ctx, int arg1, float arg2,
            [Option] bool opt1 = true, [Option(ShortName = 'o')] bool opt2 = false)
        {
            return $"{arg1} {arg2} {opt1} {opt2}";
        }
    }

    [Fact]
    public async Task TestCommands()
    {
        var adapter = new MockAdapter();
        var channelClient = adapter.GetChannelClient();
        var friendClient = adapter.GetFriendClient();

        var builder = FlandreApp.CreateBuilder();
        using var app = builder
            .AddAdapter(adapter)
            .AddPlugin<TestPlugin>()
            .Build();

        await app.StartWithDefaultsAsync();

        MessageContent? content;

        content = await channelClient.SendMessageForReply("OMR:114514");
        Assert.Equal("OMR:114514", content?.GetText());

        content = await friendClient.SendMessageForReply("test1 --opt 114.514 true");
        Assert.Equal("True 314.514", content?.GetText());

        content = await friendClient.SendMessageForReply("test2  -o 123 191.981  --no-opt1");
        Assert.Equal("123 191.981 False True",
            content?.GetText());
    }

    [Fact]
    public void TestInformalAttributes()
    {
        var builder = FlandreApp.CreateBuilder();
        using var app = builder
            .AddPlugin<TestPlugin>()
            .Build();

        Assert.Equal("This is a test command.",
            app.RootCommandNode.FindSubNode("test1")?.Command?.Description);

        Assert.True(app.RootCommandNode.FindSubNode("test2")?.Command?.IsObsolete);
        Assert.Equal("This command is obsoleted.",
            app.RootCommandNode.FindSubNode("test2")?.Command?.ObsoleteMessage);
    }
}
