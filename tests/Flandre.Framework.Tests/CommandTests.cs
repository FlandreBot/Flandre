using System.ComponentModel;
using System.Text;
using Flandre.Core.Common;
using Flandre.Framework.Routing;
using Flandre.Framework.Services;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable UnusedMember.Local

namespace Flandre.Framework.Tests;

public class CommandTests
{
    private sealed class TestPlugin : Plugin
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
    public void TestCommands()
    {
        using var app = Utils.StartTestApp<TestPlugin>(out var client);

        var service = app.Services.GetRequiredService<CommandService>();

        Assert.Equal(7, service.RootNode.CountCommands());

        MessageContent? content;

        content = client.SendMessageForReply("OMR:114514");
        Assert.Equal("OMR:114514", content?.GetText());

        content = client.SendMessageForReply("test1 --opt 114.514 true");
        Assert.Equal("True 314.514", content?.GetText());
        //
        content = client.SendMessageForReply("test2  -o 123 191.981  --no-opt1");
        Assert.Equal("123 191.981 False True",
            content?.GetText());

        // test async
        content = client.SendMessageForReply("testasync");
        Assert.Equal("ok!", content?.GetText());
    }

    [Fact]
    public void TestShortcuts()
    {
        using var app = Utils.StartTestApp<TestPlugin>(out var client);

        MessageContent? content;

        content = client.SendMessageForReply("测3试");
        Assert.Equal("3 someStr", content?.GetText());

        content = client.SendMessageForReply("测试4");
        Assert.Equal("123.456", content?.GetText());

        content = client.SendMessageForReply("测试4 114.514", TimeSpan.FromSeconds(2));
        Assert.Null(content?.GetText());

        content = client.SendMessageForReply("测试5 333");
        Assert.Equal("111.222 333 True", content?.GetText());
    }

    [Fact]
    public void TestMapCommand()
    {
        using var app = Utils.StartTestApp(out var client);

        app.MapCommand("test1", (int a, int b) => a + b);
        app.MapCommand("test2.sub", (int x) => Math.Pow(x, 2));

        var content = client.SendMessageForReply("test1 123 456");
        Assert.Equal("579", content?.GetText());

        content = client.SendMessageForReply("test2 sub 12");
        Assert.Equal("144", content?.GetText());
    }

    [Fact]
    public void TestArrayParameter()
    {
        using var app = Utils.StartTestApp<TestPlugin>(out var client);

        var content = client.SendMessageForReply("test6 1.23 aaa bbb ccc  ");
        Assert.Equal("1.23 | aaa,bbb,ccc", content?.GetText());
    }

    [Fact]
    public void TestInformalAttributes()
    {
        using var app = Utils.StartTestApp<TestPlugin>(out _);

        var cmdService = app.Services.GetRequiredService<CommandService>();

        Assert.Equal("This is a test command.",
            cmdService.RootNode.FindSubNode("test1")?.Command?.Description);

        Assert.True(cmdService.RootNode.FindSubNode("test2")?.Command?.IsObsolete);
        Assert.Equal("This command is obsoleted.",
            cmdService.RootNode.FindSubNode("test2")?.Command?.ObsoleteMessage);
    }
}
