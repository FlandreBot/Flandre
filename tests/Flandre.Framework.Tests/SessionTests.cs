using Flandre.Framework.Extensions;

namespace Flandre.Framework.Tests;

public class SessionTests
{
    private class TestPlugin : Plugin
    {
        [Command("start-session")]
        public static async Task<MessageContent?> OnStartSession(CommandContext ctx)
        {
            var nextMsg = await ctx.StartSessionAsync(TimeSpan.FromSeconds(2));
            return nextMsg?.Content;
        }
    }

    [Fact]
    public async Task TestCommandSession()
    {
        var adapter = new MockAdapter();
        var client = adapter.GetChannelClient();
        var builder = FlandreApp.CreateBuilder();
        builder.Adapters.Add(adapter);
        builder.Plugins.Add<TestPlugin>();
        using var app = builder.Build();

        await app.StartWithDefaultsAsync();

        var task1 = client.SendMessageForReply("start-session");
        await Task.Delay(TimeSpan.FromSeconds(1));
        client.SendMessage("return this");
        Assert.NotNull(await task1);

        var task2 = client.SendMessageForReply("start-session");
        await Task.Delay(TimeSpan.FromSeconds(3));
        client.SendMessage("timeout!");
        Assert.Null(await task2);
    }
}
