using Flandre.Adapters.Mock;

// ReSharper disable StringLiteralTypo

namespace Flandre.Framework.Tests;

public class FlandreAppTests
{
    [Fact]
    public async Task TestShortcutAndAlias()
    {
        var adapter = new MockAdapter();
        var channelClient = adapter.GetChannelClient();

        var builder = FlandreApp.CreateBuilder();
        using var app = builder
            .AddAdapter(adapter)
            .AddPlugin<TestPlugin>()
            .Build();

        await app.StartAsync();

        Assert.Equal(9, app.RootCommandNode.CountCommands());
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
        Assert.Equal(app.RootCommandNode.GetNodeByPath("sssuuubbb"),
            app.RootCommandNode.GetNodeByPath("sub.test"));

        const string result = "arg1: True opt: 114.514 b: False t: True";

        var content = await channelClient.SendMessageForReply("test1 true --opt 114.514");
        Assert.Equal(result, content?.GetText());

        content = await channelClient.SendMessageForReply(" 测试  true --opt 114.514");
        Assert.Equal(result, content?.GetText());

        await app.StopAsync();
    }
}