using Flandre.Adapters.Mock;
using Flandre.Core.Messaging;

namespace Flandre.Framework.Tests;

public class CommandTests
{
    [Fact]
    public async Task TestCommands()
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

        content = await channelClient.SendMessageForReply("OMR:114514");
        Assert.Equal("OMR:114514", content?.GetText());

        content = await friendClient.SendMessageForReply("test1 --opt 114.514 true");
        Assert.Equal("True 314.514", content?.GetText());

        content = await friendClient.SendMessageForReply("test2  -o 123 191.981  --no-opt1");
        Assert.Equal("123 191.981 False True",
            content?.GetText());

        // content = await friendClient.SendMessageForReply("test1 -bo 111.444 --no-trueopt false");
        // Assert.Equal("arg1: False opt: 111.444 b: True t: False",
        //     content?.GetText());
        //
        // content = await friendClient.SendMessageForReply("test2  some text aaa   bbb   ");
        // Assert.Equal("some text aaa   bbb",
        //     content?.GetText());
    }
}