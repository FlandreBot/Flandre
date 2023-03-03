using System.ComponentModel;
using Flandre.Framework.Services;
using Microsoft.Extensions.DependencyInjection;

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
            return new MessageBuilder()
                .Text($"{arg1} {opt + 200}")
                .Build();
        }

        [Command("test2", "..test111.11...45.14.")]
        [Obsolete("This command is obsoleted.")]
        public static string OnTest2(CommandContext ctx, int arg1, float arg2,
            [Option] bool opt1 = true, [Option(ShortName = 'o')] bool opt2 = false)
        {
            return $"{arg1} {arg2} {opt1} {opt2}";
        }

        [Command("test3")]
        [RegexShortcut("测([0-9A-Za-z_])试", "$1 someStr")]
        public static string OnTest3(CommandContext ctx, string arg1, string arg2)
        {
            return $"{arg1} {arg2}";
        }

        [Command("test4")]
        [StringShortcut("测试4", "123.456")]
        public static string OnTest4(CommandContext ctx, double arg1)
        {
            return $"{arg1}";
        }

        [Command("test5")]
        [StringShortcut("测试5", "111.222 --opt1", AllowArguments = true)]
        public static string OnTest5(CommandContext ctx, double arg1, int arg2, [Option] bool opt1)
        {
            return $"{arg1} {arg2} {opt1}";
        }
    }

    [Fact]
    public async Task TestCommands()
    {
        var adapter = new MockAdapter();
        var channelClient = adapter.GetChannelClient();
        var friendClient = adapter.GetFriendClient();

        var builder = FlandreApp.CreateBuilder();
        var app = builder
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

        await app.StopAsync();
    }

    [Fact]
    public async Task TestShortcuts()
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

        content = await channelClient.SendMessageForReply("测3试");
        Assert.Equal("3 someStr", content?.GetText());

        content = await friendClient.SendMessageForReply("测试4");
        Assert.Equal("123.456", content?.GetText());

        content = await friendClient.SendMessageForReply("测试4 114.514", TimeSpan.FromSeconds(2));
        Assert.Null(content?.GetText());

        content = await friendClient.SendMessageForReply("测试5 333");
        Assert.Equal("111.222 333 True",
            content?.GetText());
    }

    [Fact]
    public void TestInformalAttributes()
    {
        var builder = FlandreApp.CreateBuilder();
        using var app = builder
            .AddPlugin<TestPlugin>()
            .Build();

        var cmdService = app.Services.GetRequiredService<CommandService>();

        Assert.Equal("This is a test command.",
            cmdService.RootCommandNode.FindSubNode("test1")?.Command?.Description);

        Assert.True(cmdService.RootCommandNode.FindSubNode("test2")?.Command?.IsObsolete);
        Assert.Equal("This command is obsoleted.",
            cmdService.RootCommandNode.FindSubNode("test2")?.Command?.ObsoleteMessage);
    }
}
