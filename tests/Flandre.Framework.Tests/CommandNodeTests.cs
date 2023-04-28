using Flandre.Framework.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Flandre.Framework.Tests;

public class CommandNodeTests
{
    private class TestPlugin : Plugin
    {
        [Command("cmd-xxx")]
        public static MessageContent? ACommandInRoot() => null;

        [Command("...cmd-aaa.cmd-bbb...")]
        public static MessageContent? ASubCommand() => null;

        [Command(".cmd-bbb")]
        public static MessageContent? AnotherCommandInRoot() => null;
    }

    [Fact]
    public async Task TestNode()
    {
        var builder = FlandreApp.CreateBuilder();
        builder.Plugins.Add<TestPlugin>();
        using var app = builder.Build();

        await app.StartWithDefaultsAsync();

        var cmdService = app.Services.GetRequiredService<CommandService>();

        Assert.Equal(3, cmdService.RootCommandNode.CountCommands());
        Assert.Equal("cmd-xxx", cmdService.RootCommandNode.FindSubNode("cmd-xxx")?.FullName);
        Assert.Equal("cmd-aaa.cmd-bbb", cmdService.RootCommandNode.FindSubNode("cmd-aaa..cmd-bbb")?.FullName);
        Assert.Equal("cmd-aaa.cmd-bbb", cmdService.RootCommandNode.FindSubNode("cmd-aaa..cmd-bbb")?.Command?.FullName);
        Assert.Equal("cmd-bbb", cmdService.RootCommandNode.FindSubNode("cmd-bbb . .")?.Command?.FullName);

        await app.StopAsync();
    }
}
