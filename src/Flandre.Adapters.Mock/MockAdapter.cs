using Flandre.Core.Common;

#pragma warning disable CS1998

namespace Flandre.Adapters.Mock;

public class MockAdapter : IAdapter
{
    internal readonly MockBot Bot = new();

    public Task StartAsync() => Task.CompletedTask;

    public Task StopAsync() => Task.CompletedTask;

    public IEnumerable<Bot> GetBots()
    {
        return new[] { Bot };
    }
}
