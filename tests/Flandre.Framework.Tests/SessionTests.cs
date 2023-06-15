using Flandre.Framework.Routing;

namespace Flandre.Framework.Tests;

public class SessionTests
{
    private sealed class TestPlugin : Plugin
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
        await using var app = Utils.StartTestApp<TestPlugin>(out var client);

        var task1 = client.SendMessageForReplyAsync("start-session");
        await Task.Delay(TimeSpan.FromSeconds(1));
        client.SendMessage("return this");
        Assert.NotNull(await task1);

        var task2 = client.SendMessageForReplyAsync("start-session");
        await Task.Delay(TimeSpan.FromSeconds(3));
        client.SendMessage("timeout!");
        Assert.Null(await task2);
    }
}
