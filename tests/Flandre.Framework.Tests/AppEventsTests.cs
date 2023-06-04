using Flandre.Framework.Routing;

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
        builder.Adapters.Add(adapter);
        builder.Plugins.Add<TestPlugin>();
        using var app = builder.Build();

        var count = 0;
        string? cmdName = null;
        Exception? ex = null;

        app.Starting += (_, _) => count += 1;
        app.Ready += (_, _) =>
        {
            count += 10;
            client.SendMessage("throw-ex");
        };
        app.Stopped += (_, _) => count += 100;

        app.CommandInvoking += (_, e) => cmdName = e.Command.Name;
        app.CommandInvoked += (_, e) => { ex = e.Exception; };

        await app.StartWithDefaultsAsync();
        await Task.Delay(TimeSpan.FromSeconds(1));
        await app.StopAsync();

        Assert.Equal(111, count);
        Assert.Equal("throw-ex", cmdName);
        Assert.Equal("Test Exception", ex?.Message);
    }
}
