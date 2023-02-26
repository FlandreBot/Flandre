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
            .AddAdapter(adapter)
            .Build();

        // Order Note
        //  1,2,3  ↓     ↑  2
        // [Custom Middleware #1]
        //      2  ↓     ↑  2
        // [Custom Middleware #2]

        int count1In = 0, count1Out = 0, count2 = 0;

        // Custom Middleware #1
        app.UseMiddleware(async (ctx, next) =>
        {
            // 1, 2, 3 pass through

            if (ctx.Message.GetText().Contains("(3)"))
                // 3 shorts here
                return;

            count1In++;

            if (ctx.Message.GetText().Contains("(1)"))
            {
                ctx.Response = "ok";
                // 1 shorts here
                return;
            }

            // only 2 passes through the next middleware
            await next();
            // 2 goes out

            count1Out++;
        });

        // Custom Middleware #2
        app.UseMiddleware(async (ctx, next) =>
        {
            // 2 passes through
            count2++;

            ctx.Response = ctx.Message.Content;

            // 2 goes out
            await next();
        });

        await app.StartAsync();

        var content1 = await client.SendMessageForReply("test (1) short me at middleware #1");
        Assert.NotNull(content1);

        var content2 = await client.SendMessageForReply("test (2) pass me through all middleware");
        Assert.NotNull(content2);

        var content3 = await client.SendMessageForReply("test (3) don't pass me", TimeSpan.FromSeconds(2));
        Assert.Null(content3);

        await app.StopAsync();

        Assert.Equal(2, count1In);
        Assert.Equal(1, count2);
        Assert.Equal(1, count1Out);
    }
}
