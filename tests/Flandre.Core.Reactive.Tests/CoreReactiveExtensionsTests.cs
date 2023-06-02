using System.Reactive.Linq;
using Flandre.Adapters.Mock;

namespace Flandre.Core.Reactive.Tests;

public class CoreReactiveExtensionsTests
{
    [Fact]
    public void TestObserveMessageReceived()
    {
        var adapter = new MockAdapter();
        var client1 = adapter.GetChannelClient("testG1", "testC1", "123");
        var client2 = adapter.GetChannelClient("testG2", "testC2", "456");
        var bot = adapter.Bots.First();

        var messageCountFrom1 = 0;
        var messageCountFrom2 = 0;
        var allMessageReceived = 0;

        bot.OnMessageReceived()
            .OfUser("123")
            .Subscribe(_ => messageCountFrom1++);

        bot.OnMessageReceived()
            .Select(e => e.Message)
            .OfGuild("testG1")
            .Subscribe(_ => messageCountFrom1++);

        bot.OnMessageReceived()
            .Select(e => e.Message)
            .OfUser("456")
            .Subscribe(_ => messageCountFrom2++);

        bot.OnMessageReceived()
            .OfChannel("testC2")
            .Subscribe(_ => messageCountFrom2++);

        bot.OnMessageReceived()
            .Subscribe(_ => allMessageReceived++);


        client1.SendMessage("abc");
        client1.SendMessage("abc");
        client2.SendMessage("abc");

        Assert.Equal(4, messageCountFrom1);
        Assert.Equal(2, messageCountFrom2);
        Assert.Equal(3, allMessageReceived);
    }
}
