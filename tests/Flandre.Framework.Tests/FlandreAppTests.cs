using Flandre.Adapters.Mock;

// ReSharper disable StringLiteralTypo

namespace Flandre.Framework.Tests;

public class FlandreAppTests
{
    [Fact]
    public void TestFlandreApp()
    {
        var builder = new FlandreAppBuilder();
        var adapter = new MockAdapter();

        var channelClient = adapter.GetChannelClient();
        var friendClient = adapter.GetFriendClient();

        var app = builder
            .UseAdapter(adapter)
            .UsePlugin<TestPlugin>()
            .Build();

        app.OnReady += (_, _) =>
        {
            var content = channelClient.SendForReply("OMR:114514").Result;
            Assert.Equal("OMR:114514", content?.GetText());
            // throw new Exception();

            content = friendClient.SendForReply("test1 true --opt 114.514").Result;
            Assert.Equal("arg1: True opt: 114.514 b: False t: True",
                content?.GetText());

            content = friendClient.SendForReply("test1  -o 1919.810  false").Result;
            Assert.Equal("arg1: False opt: 1919.81 b: False t: True",
                content?.GetText());

            content = friendClient.SendForReply("test1 -bo 111.444 --no-trueopt false").Result;
            Assert.Equal("arg1: False opt: 111.444 b: True t: False",
                content?.GetText());

            content = friendClient.SendForReply("test2  some text aaa   bbb   ").Result;
            Assert.Equal("some text aaa   bbb",
                content?.GetText());

            app.Stop();
        };

        app.Run();
    }

    [Fact]
    public void TestShortcutAndAlias()
    {
        var builder = new FlandreAppBuilder();
        var adapter = new MockAdapter();

        var channelClient = adapter.GetChannelClient();

        var app = builder
            .UseAdapter(adapter)
            .UsePlugin<TestPlugin>()
            .Build();

        app.OnReady += (_, _) =>
        {
            Assert.Equal(7, app.CommandMap.Count);
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

            var content = channelClient.SendForReply("test1 true --opt 114.514").Result;
            Assert.Equal(result, content?.GetText());

            content = channelClient.SendForReply(" 测试  true --opt 114.514").Result;
            Assert.Equal(result, content?.GetText());

            app.Stop();
        };

        app.Run();
    }
}