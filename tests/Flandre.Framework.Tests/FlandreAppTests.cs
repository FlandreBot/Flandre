// ReSharper disable StringLiteralTypo

using Flandre.Framework.Services;
using Microsoft.Extensions.DependencyInjection;

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
    public void TestAliases()
    {
        var builder = FlandreApp.CreateBuilder();
        using var app = builder
            .AddPlugin<TestPlugin>()
            .Build();

        var cmdService = app.Services.GetRequiredService<CommandService>();

        Assert.Equal(3, cmdService.RootCommandNode.CountCommands());

        Assert.NotNull(cmdService.RootCommandNode.FindSubNode("test"));
        Assert.NotNull(cmdService.RootCommandNode.FindSubNode("sub.test"));
        Assert.NotNull(cmdService.RootCommandNode.FindSubNode("sub.sub.sub.test"));

        // alias
        Assert.NotNull(cmdService.RootCommandNode.FindSubNode("test111.11.45.14"));
        Assert.Equal(cmdService.RootCommandNode.FindSubNode("sssuuubbb")?.Command,
            cmdService.RootCommandNode.FindSubNode("sub.test")?.Command);
    }

    [Fact]
    public void TestShortcutCount()
    {
        var builder = FlandreApp.CreateBuilder();
        using var app = builder
            .AddPlugin<TestPlugin>()
            .Build();

        var cmdService = app.Services.GetRequiredService<CommandService>();

        Assert.Equal(2, cmdService.StringShortcuts.Count);
        Assert.Single(cmdService.RegexShortcuts);
    }
}
