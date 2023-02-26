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

        Assert.Equal(3, app.RootCommandNode.CountCommands());
        Assert.Equal("cmd-xxx", app.RootCommandNode.FindSubNode("cmd-xxx")?.FullName);
        Assert.Equal("cmd-aaa.cmd-bbb", app.RootCommandNode.FindSubNode("cmd-aaa..cmd-bbb")?.FullName);
        Assert.Equal("cmd-aaa.cmd-bbb", app.RootCommandNode.FindSubNode("cmd-aaa..cmd-bbb")?.Command?.FullName);
        Assert.Equal("cmd-bbb", app.RootCommandNode.FindSubNode("cmd-bbb . .")?.Command?.FullName);
    }
}
