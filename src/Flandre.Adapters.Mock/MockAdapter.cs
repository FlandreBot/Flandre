using Flandre.Core.Common;

#pragma warning disable CS1998

namespace Flandre.Adapters.Mock;

public class MockAdapter : IAdapter
{
    public IEnumerable<Bot> Bots => new[] { Bot };

    internal readonly MockBot Bot = new();

    public Task StartAsync() => Task.CompletedTask;

    public Task StopAsync() => Task.CompletedTask;
}
