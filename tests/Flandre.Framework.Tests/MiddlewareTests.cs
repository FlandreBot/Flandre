using Flandre.Adapters.Mock;

namespace Flandre.Framework.Tests;

public class MiddlewareTests
{
    [Fact]
    public async Task TestMiddleware()
    {
        var adapter = new MockAdapter();
        var client = adapter.GetChannelClient();

        var builder = FlandreApp.CreateBuilder();
        using var app = builder
            .UseAdapter(adapter)
            .UsePlugin<TestPlugin>()
            .Build();

        // Order Note
        //  1,2,3  ↓     ↑  2
        // [Internal Middlewares]
        //  1,2,3  ↓     ↑  2
        // [Custom Middleware #1]
        //      2  ↓     ↑  2
        // [Custom Middleware #2]

        int count1In = 0, count1Out = 0, count2 = 0;

        // Custom Middleware #1
        app.UseMiddleware((ctx, next) =>
        {
            // 1, 2, 3 pass through

            if (ctx.Message.GetText() == "test (3) don't pass me")
                // 3 shorts here
                return;

            count1In++;

            if (ctx.Message.GetText() == "test (1) short me at middleware #1")
            {
                ctx.Response = "ok";
                // 1 shorts here
                return;
            }

            // only 2 passes through the next middleware
            next();
            // 2 goes out

            count1Out++;
        });

        // Custom Middleware #2
        app.UseMiddleware((_, next) =>
        {
            // 2 passes through
            count2++;

            // 2 goes out
            next();
        });

        await app.StartAsync();

        var content1 = await client.SendForReply("test (1) short me at middleware #1");
        Assert.NotNull(content1);

        var content2 = await client.SendForReply("test (2) pass me through all middlewares");
        Assert.NotNull(content2);

        var content3 = await client.SendForReply("test (3) don't pass me", TimeSpan.FromSeconds(2));
        Assert.Null(content3);

        await app.StopAsync();

        Assert.Equal(2, count1In);
        Assert.Equal(1, count2);
        Assert.Equal(1, count1Out);
    }
}