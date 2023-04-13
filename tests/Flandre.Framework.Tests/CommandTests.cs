using System.ComponentModel;
using System.Text;
using Flandre.Core.Common;
using Flandre.Framework.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Flandre.Framework.Tests;

public class CommandTests
{
    public class TestPlugin : Plugin
    {
        public override async Task OnMessageReceivedAsync(MessageContext ctx)
        {
            if (ctx.Message.GetText().StartsWith("OMR:"))
                await ctx.Bot.SendMessageAsync(ctx.Message);
        }

        [Command]
        [Description("This is a test command.")]
        public static MessageContent Test1(bool arg1, [Option] double opt = 0)
        {
            return new MessageBuilder()
                .Text($"{arg1} {opt + 200}")
                .Build();
        }

        [Command("test2", "..test111.11...45.14.")]
        [Obsolete("This command is obsoleted.")]
        public static string Test2(int arg1, float arg2, CommandContext ctx,
            [Option] bool opt1 = true, [Option(ShortName = 'o')] bool opt2 = false)
        {
            return $"{arg1} {arg2} {opt1} {opt2}";
        }

        [Command]
        [RegexShortcut("测([0-9A-Za-z_])试", "$1 someStr")]
        public static string Test3(string arg1, string arg2)
        {
            return $"{arg1} {arg2}";
        }

        [Command]
        [StringShortcut("测试4", "123.456")]
        public static string Test4(double arg1)
        {
            return $"{arg1}";
        }

        [Command]
        [StringShortcut("测试5", "111.222 --opt1", AllowArguments = true)]
        public static string Test5(double arg1, int arg2, [Option] bool opt1)
        {
            return $"{arg1} {arg2} {opt1}";
        }

        // Array parameter
        [Command]
        public static string Test6(double arg, params string[] strArr)
        {
            return new StringBuilder()
                .Append(arg)
                .Append(" | ")
                .Append(string.Join(',', strArr))
                .ToString();
        }

        [Command]
        public static async ValueTask<string> TestAsync()
        {
            // simulates async tasks
            await Task.Run(() => { });
            return "ok!";
        }
    }

    [Fact]
    public async Task TestCommands()
    {
        var adapter = new MockAdapter();
        var channelClient = adapter.GetChannelClient();
        var friendClient = adapter.GetFriendClient();

        var builder = FlandreApp.CreateBuilder();
        builder.Adapters.Add(adapter);
        builder.Plugins.Add<TestPlugin>();
        using var app = builder.Build();

        var service = app.Services.GetRequiredService<CommandService>();

        Assert.Equal(7, service.RootCommandNode.CountCommands());

        await app.StartWithDefaultsAsync();

        MessageContent? content;

        content = await channelClient.SendMessageForReply("OMR:114514");
        Assert.Equal("OMR:114514", content?.GetText());

        content = await friendClient.SendMessageForReply("test1 --opt 114.514 true");
        Assert.Equal("True 314.514", content?.GetText());

        content = await friendClient.SendMessageForReply("test2  -o 123 191.981  --no-opt1");
        Assert.Equal("123 191.981 False True",
            content?.GetText());

        // test async
        content = await friendClient.SendMessageForReply("testasync");
        Assert.Equal("ok!", content?.GetText());

        await app.StopAsync();
    }

    [Fact]
    public async Task TestShortcuts()
    {
        var adapter = new MockAdapter();
        var channelClient = adapter.GetChannelClient();
        var friendClient = adapter.GetFriendClient();

        var builder = FlandreApp.CreateBuilder();
        builder.Adapters.Add(adapter);
        builder.Plugins.Add<TestPlugin>();
        using var app = builder.Build();

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
    public async Task TestArrayParameter()
    {
        var adapter = new MockAdapter();
        var client = adapter.GetFriendClient();

        var builder = FlandreApp.CreateBuilder();
        builder.Adapters.Add(adapter);
        builder.Plugins.Add<TestPlugin>();
        using var app = builder.Build();

        await app.StartWithDefaultsAsync();

        var content = await client.SendMessageForReply("test6 1.23 aaa bbb ccc  ");
        Assert.Equal("1.23 | aaa,bbb,ccc", content?.GetText());

        await app.StopAsync();
    }

    [Fact]
    public void TestInformalAttributes()
    {
        var builder = FlandreApp.CreateBuilder();
        builder.Plugins.Add<TestPlugin>();
        using var app = builder.Build();

        var cmdService = app.Services.GetRequiredService<CommandService>();

        Assert.Equal("This is a test command.",
            cmdService.RootCommandNode.FindSubNode("test1")?.Command?.Description);

        Assert.True(cmdService.RootCommandNode.FindSubNode("test2")?.Command?.IsObsolete);
        Assert.Equal("This command is obsoleted.",
            cmdService.RootCommandNode.FindSubNode("test2")?.Command?.ObsoleteMessage);
    }
}
