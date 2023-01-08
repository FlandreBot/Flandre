using Flandre.Adapters.Mock;
using Flandre.Core.Messaging;

// ReSharper disable StringLiteralTypo

namespace Flandre.Framework.Tests;

public class FlandreAppTests
{
    [Fact]
    public async Task TestFlandreApp()
    {
        var adapter = new MockAdapter();
        var channelClient = adapter.GetChannelClient();
        var friendClient = adapter.GetFriendClient();

        var builder = FlandreApp.CreateBuilder();
        using var app = builder
            .AddAdapter(adapter)
            .AddPlugin<TestPlugin>()
            .Build();

        await app.StartAsync();

        MessageContent? content;

        content = await channelClient.SendForReply("OMR:114514");
        Assert.Equal("OMR:114514", content?.GetText());
        // throw new Exception();

        content = await friendClient.SendForReply("test1 true --opt 114.514");
        Assert.Equal("arg1: True opt: 114.514 b: False t: True",
            content?.GetText());

        content = await friendClient.SendForReply("test1  -o 1919.810  false");
        Assert.Equal("arg1: False opt: 1919.81 b: False t: True",
            content?.GetText());

        content = await friendClient.SendForReply("test1 -bo 111.444 --no-trueopt false");
        Assert.Equal("arg1: False opt: 111.444 b: True t: False",
            content?.GetText());

        content = await friendClient.SendForReply("test2  some text aaa   bbb   ");
        Assert.Equal("some text aaa   bbb",
            content?.GetText());

        await app.StopAsync();
    }

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

        Assert.Equal(8, app.CommandMap.Count);
        Assert.Equal(2, app.ShortcutMap.Count);

        Assert.NotNull(app.CommandMap.GetValueOrDefault("test1"));
        Assert.NotNull(app.CommandMap.GetValueOrDefault("sub.test"));
        Assert.NotNull(app.CommandMap.GetValueOrDefault("sub.sub.sub.test"));

        // shortcut
        Assert.NotNull(app.ShortcutMap.GetValueOrDefault("测试"));
        Assert.NotNull(app.ShortcutMap.GetValueOrDefault("子测试"));
        Assert.Equal(app.ShortcutMap["测试"], app.CommandMap["test1"]);

        // alias
        Assert.NotNull(app.CommandMap.GetValueOrDefault("test111.11.45.14"));
        Assert.Equal(app.CommandMap["sssuuubbb"], app.CommandMap["sub.test"]);

        var result = "arg1: True opt: 114.514 b: False t: True";

        var content = await channelClient.SendForReply("test1 true --opt 114.514");
        Assert.Equal(result, content?.GetText());

        content = await channelClient.SendForReply(" 测试  true --opt 114.514");
        Assert.Equal(result, content?.GetText());

        await app.StopAsync();
    }
}