using Flandre.Framework.Routing;
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
        using var app = Utils.StartTestApp<TestPlugin>(out _);

        var cmdService = app.Services.GetRequiredService<CommandService>();

        Assert.Equal(3, cmdService.RootNode.CountCommands());
        Assert.Equal("cmd-xxx", cmdService.RootNode.FindSubNode("cmd-xxx")?.FullName);
        Assert.Equal("cmd-aaa.cmd-bbb", cmdService.RootNode.FindSubNode("cmd-aaa..cmd-bbb")?.FullName);
        Assert.Equal("cmd-aaa.cmd-bbb", cmdService.RootNode.FindSubNode("cmd-aaa..cmd-bbb")?.Command?.FullName);
        Assert.Equal("cmd-bbb", cmdService.RootNode.FindSubNode("cmd-bbb . .")?.Command?.FullName);
    }
}
