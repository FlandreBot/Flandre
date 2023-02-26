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
        public static MessageContent? OnSubSubSubTest(CommandContext ctx) => null;
    }
    
    [Fact]
    public async Task TestShortcutAndAlias()
    {
        var builder = FlandreApp.CreateBuilder();
        using var app = builder
            .AddPlugin<TestPlugin>()
            .Build();

        await app.StartAsync();

        Assert.Equal(3, app.RootCommandNode.CountCommands());
        Assert.Equal(2, app.StringShortcuts.Count);

        Assert.NotNull(app.RootCommandNode.FindSubNode("test"));
        Assert.NotNull(app.RootCommandNode.FindSubNode("sub.test"));
        Assert.NotNull(app.RootCommandNode.FindSubNode("sub.sub.sub.test"));

        // shortcut
        Assert.NotNull(app.StringShortcuts.GetValueOrDefault("测试"));
        Assert.NotNull(app.StringShortcuts.GetValueOrDefault("子测试"));
        Assert.Equal(app.StringShortcuts["测试"],
            app.RootCommandNode.FindSubNode("test")?.Command);

        // alias
        Assert.NotNull(app.RootCommandNode.FindSubNode("test111.11.45.14"));
        Assert.Equal(app.RootCommandNode.FindSubNode("sssuuubbb")?.Command,
            app.RootCommandNode.FindSubNode("sub.test")?.Command);

        await app.StopAsync();
    }
}
