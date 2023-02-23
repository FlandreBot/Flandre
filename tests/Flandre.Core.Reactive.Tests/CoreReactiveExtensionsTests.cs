using System.Reactive.Linq;
using Flandre.Adapters.Mock;

namespace Flandre.Core.Reactive.Tests;

public class CoreReactiveExtensionsTests
{
    [Fact]
    public void TestObserveMessageReceived()
    {
        var adapter = new MockAdapter();
        var client1 = adapter.GetChannelClient("test", "test", "123");
        var client2 = adapter.GetChannelClient("test", "test", "456");
        var bot = adapter.GetBots().ToArray()[0];

        var messageCountFrom1 = 0;
        var messageCountFrom2 = 0;
        var messageCountFromAll = 0;

        bot.ObserveMessageReceived()
            .Where(e => e.Message.Sender.UserId == "123")
            .Subscribe(_ => messageCountFrom1++);

        bot.ObserveMessageReceived()
            .Where(e => e.Message.Sender.UserId == "456")
            .Subscribe(_ => messageCountFrom2++);

        bot.ObserveMessageReceived()
            .Subscribe(_ => messageCountFromAll++);

        client1.SendMessage("abc");
        client1.SendMessage("abc");
        client2.SendMessage("abc");

        Assert.Equal(2, messageCountFrom1);
        Assert.Equal(1, messageCountFrom2);
        Assert.Equal(3, messageCountFromAll);
    }
}
