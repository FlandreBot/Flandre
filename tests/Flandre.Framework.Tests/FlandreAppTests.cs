// ReSharper disable StringLiteralTypo

using Flandre.Framework.Routing;
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
        using var app = Utils.StartTestApp<TestPlugin>(out _);

        var cmdService = app.Services.GetRequiredService<CommandService>();

        Assert.Equal(3, cmdService.RootNode.CountCommands());

        Assert.NotNull(cmdService.RootNode.FindSubNode("test"));
        Assert.NotNull(cmdService.RootNode.FindSubNode("sub.test"));
        Assert.NotNull(cmdService.RootNode.FindSubNode("sub.sub.sub.test"));

        // alias
        Assert.NotNull(cmdService.RootNode.FindSubNode("test111.11.45.14"));
        Assert.Equal(cmdService.RootNode.FindSubNode("sssuuubbb")?.Command,
            cmdService.RootNode.FindSubNode("sub.test")?.Command);
    }

    [Fact]
    public void TestShortcutCount()
    {
        using var app = Utils.StartTestApp<TestPlugin>(out _);

        var cmdService = app.Services.GetRequiredService<CommandService>();

        Assert.Equal(2, cmdService.StringShortcuts.Count);
        Assert.Single(cmdService.RegexShortcuts);
    }
}
