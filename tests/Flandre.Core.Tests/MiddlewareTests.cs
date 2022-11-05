using Flandre.Adapters.Mock;
using Flandre.Core.Utils;

namespace Flandre.Core.Tests;

public class MiddlewareTests
{
    [Fact]
    public void TestMiddleware()
    {
        Logger.ThrowOnError = true;
        var app = new FlandreApp();
        var adapter = new MockAdapter();

        app.Use((ctx, next) =>
        {
            if (ctx.Message.GetText() == "OMR:pass me")
                next();
        });

        var client = adapter.GetChannelClient();

        app.OnAppReady += (_, _) =>
        {
            var content1 = client.SendForReply("OMR:pass me").Result;
            Assert.NotNull(content1);

            var content2 = client.SendForReply("don't pass me", new TimeSpan(0, 0, 2)).Result;
            Assert.Null(content2);

            app.Stop();
        };

        app.Use(adapter).Use(new TestPlugin()).Start();
    }
}