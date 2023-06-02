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
    public async Task TestAliases()
    {
        var builder = FlandreApp.CreateBuilder();
        builder.Plugins.Add<TestPlugin>();
        using var app = builder.Build();

        await app.StartWithDefaultsAsync();

        var cmdService = app.Services.GetRequiredService<CommandService>();

        Assert.Equal(3, cmdService.RootNode.CountCommands());

        Assert.NotNull(cmdService.RootNode.FindSubNode("test"));
        Assert.NotNull(cmdService.RootNode.FindSubNode("sub.test"));
        Assert.NotNull(cmdService.RootNode.FindSubNode("sub.sub.sub.test"));

        // alias
        Assert.NotNull(cmdService.RootNode.FindSubNode("test111.11.45.14"));
        Assert.Equal(cmdService.RootNode.FindSubNode("sssuuubbb")?.Command,
            cmdService.RootNode.FindSubNode("sub.test")?.Command);

        await app.StopAsync();
    }

    [Fact]
    public async Task TestShortcutCount()
    {
        var builder = FlandreApp.CreateBuilder();
        builder.Plugins.Add<TestPlugin>();
        using var app = builder.Build();

        await app.StartAsync();

        var cmdService = app.Services.GetRequiredService<CommandService>();

        Assert.Equal(2, cmdService.StringShortcuts.Count);
        Assert.Single(cmdService.RegexShortcuts);

        await app.StopAsync();
    }
}
