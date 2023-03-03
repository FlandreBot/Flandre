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
    public void TestNode()
    {
        var app = FlandreApp.CreateBuilder()
            .AddPlugin<TestPlugin>()
            .Build();

        var cmdService = app.Services.GetRequiredService<CommandService>();

        Assert.Equal(3, cmdService.RootCommandNode.CountCommands());
        Assert.Equal("cmd-xxx", cmdService.RootCommandNode.FindSubNode("cmd-xxx")?.FullName);
        Assert.Equal("cmd-aaa.cmd-bbb", cmdService.RootCommandNode.FindSubNode("cmd-aaa..cmd-bbb")?.FullName);
        Assert.Equal("cmd-aaa.cmd-bbb", cmdService.RootCommandNode.FindSubNode("cmd-aaa..cmd-bbb")?.Command?.FullName);
        Assert.Equal("cmd-bbb", cmdService.RootCommandNode.FindSubNode("cmd-bbb . .")?.Command?.FullName);
    }
}
