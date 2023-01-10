using Flandre.Adapters.Mock;

namespace Flandre.Framework.Tests;

public class SessionTests
{
    [Fact]
    public async Task TestCommandSession()
    {
        var adapter = new MockAdapter();
        var client = adapter.GetChannelClient();
        var builder = FlandreApp.CreateBuilder();
        using var app = builder
            .AddAdapter(adapter)
            .AddPlugin<TestPlugin>()
            .Build();

        await app.StartAsync();

        var task1 = client.SendMessageForReply("start-session");
        await Task.Delay(TimeSpan.FromSeconds(1));
        client.SendMessage("return this");
        Assert.NotNull(await task1);

        var task2 = client.SendMessageForReply("start-session");
        await Task.Delay(TimeSpan.FromSeconds(3));
        client.SendMessage("timeout!");
        Assert.Null(await task2);

        await app.StopAsync();
    }
}