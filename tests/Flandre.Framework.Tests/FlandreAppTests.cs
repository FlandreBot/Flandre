

// ReSharper disable StringLiteralTypo

namespace Flandre.Framework.Tests;

public class FlandreAppTests
{
    [Fact]
    public async Task TestShortcutAndAlias()
    {
        var builder = FlandreApp.CreateBuilder();
        using var app = builder
            .AddPlugin<TestPlugin>()
            .Build();

        await app.StartAsync();

        Assert.Equal(6, app.RootCommandNode.CountCommands());
        Assert.Equal(2, app.StringShortcuts.Count);

        Assert.NotNull(app.RootCommandNode.GetNodeByPath("test1"));
        Assert.NotNull(app.RootCommandNode.GetNodeByPath("sub.test"));
        Assert.NotNull(app.RootCommandNode.GetNodeByPath("sub.sub.sub.test"));

        // shortcut
        Assert.NotNull(app.StringShortcuts.GetValueOrDefault("测试"));
        Assert.NotNull(app.StringShortcuts.GetValueOrDefault("子测试"));
        Assert.Equal(app.StringShortcuts["测试"],
            app.RootCommandNode.GetNodeByPath("test1")?.Command);

        // alias
        Assert.NotNull(app.RootCommandNode.GetNodeByPath("test111.11.45.14"));
        Assert.Equal(app.RootCommandNode.GetNodeByPath("sssuuubbb")?.Command,
            app.RootCommandNode.GetNodeByPath("sub.test")?.Command);

        await app.StopAsync();
    }
}
