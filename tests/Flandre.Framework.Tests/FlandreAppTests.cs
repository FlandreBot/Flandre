// ReSharper disable StringLiteralTypo

namespace Flandre.Framework.Tests;

public class FlandreAppTests
{
    private class TestPlugin : Plugin
    {
        [Command("test", " ..  test111..11.45..14  . ")]
        [StringShortcut("测试")]
        public static MessageContent OnTest(CommandContext ctx, bool arg1, [Option] double opt = 0)
        {
            return $"{arg1} {opt + 200}";
        }

        [Command("sub.test", "sssuuubbb")]
        [StringShortcut("子测试")]
        public static MessageContent? OnSubTest(CommandContext ctx) => null;

        [Command("...sub....sub..sub......test..")]
        // lang=regex
        [RegexShortcut(@"\d\d\d", "$1")]
        public static MessageContent? OnSubSubSubTest(CommandContext ctx) => null;
    }

    [Fact]
    public async Task TestAliases()
    {
        var builder = FlandreApp.CreateBuilder();
        using var app = builder
            .AddPlugin<TestPlugin>()
            .Build();

        await app.StartAsync();

        Assert.Equal(3, app.RootCommandNode.CountCommands());

        Assert.NotNull(app.RootCommandNode.FindSubNode("test"));
        Assert.NotNull(app.RootCommandNode.FindSubNode("sub.test"));
        Assert.NotNull(app.RootCommandNode.FindSubNode("sub.sub.sub.test"));

        // alias
        Assert.NotNull(app.RootCommandNode.FindSubNode("test111.11.45.14"));
        Assert.Equal(app.RootCommandNode.FindSubNode("sssuuubbb")?.Command,
            app.RootCommandNode.FindSubNode("sub.test")?.Command);

        await app.StopAsync();
    }

    [Fact]
    public async Task TestShortcutCount()
    {
        var builder = FlandreApp.CreateBuilder();
        using var app = builder
            .AddPlugin<TestPlugin>()
            .Build();

        await app.StartAsync();

        Assert.Equal(2, app.StringShortcuts.Count);
        Assert.Single(app.RegexShortcuts);

        await app.StopAsync();
    }
}
