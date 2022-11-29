using Flandre.Adapters.Mock;

namespace Flandre.Framework.Tests;

public class MiddlewareTests
{
    [Fact]
    public void TestMiddleware()
    {
        var builder = new FlandreAppBuilder();
        var adapter = new MockAdapter();

        var client = adapter.GetChannelClient();

        var app = builder
            .UseAdapter(adapter)
            .UsePlugin<TestPlugin>()
            .Build();

        app.UseMiddleware((ctx, next) =>
        {
            if (ctx.Message.GetText() == "OMR:pass me")
                next();
        });

        app.OnReady += (_, _) =>
        {
            var content1 = client.SendForReply("OMR:pass me").Result;
            Assert.NotNull(content1);

            var content2 = client.SendForReply("don't pass me", new TimeSpan(0, 0, 2)).Result;
            Assert.Null(content2);

            app.Stop();
        };
        app.Run();
    }
}