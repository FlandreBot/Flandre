using Flandre.Core.Common;
using Flandre.Core.Utils;

#pragma warning disable CS1998

namespace Flandre.Adapters.Mock;

public class MockAdapter : IAdapter
{
    internal readonly MockBot Bot = new(new Logger("MockAdapter"));

    public Task Start() => Task.CompletedTask;

    public Task Stop() => Task.CompletedTask;

    public IEnumerable<Bot> GetBots()
    {
        return new[] { Bot };
    }
}