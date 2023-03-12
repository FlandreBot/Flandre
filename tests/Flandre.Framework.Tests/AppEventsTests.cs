namespace Flandre.Framework.Tests;

public class AppEventsTests
{
    private class TestPlugin : Plugin
    {
        [Command("throw-ex")]
        public static MessageContent? OnThrowEx(CommandContext ctx) =>
            throw new Exception("Test Exception");
    }

    [Fact]
    public async Task TestEvents()
    {
        var adapter = new MockAdapter();
        var client = adapter.GetChannelClient();

        var builder = FlandreApp.CreateBuilder();
        var app = builder
            .AddAdapter(adapter)
            .AddPlugin<TestPlugin>()
            .Build();

        var count = 0;
        string? cmdName = null;
        Exception? ex = null;

        app.OnStarting += (_, _) => count += 1;
        app.OnReady += (_, _) =>
        {
            count += 10;
            client.SendMessage("throw-ex");
        };
        app.OnStopped += (_, _) => count += 100;

        app.OnCommandInvoking += (_, e) => cmdName = e.Command.Name;
        app.OnCommandInvoked += (_, e) => { ex = e.Exception; };

        await app.StartWithDefaultsAsync();
        await Task.Delay(TimeSpan.FromSeconds(1));
        await app.StopAsync();

        Assert.Equal(111, count);
        Assert.Equal("throw-ex", cmdName);
        Assert.Equal("Test Exception", ex?.Message);
    }
}
