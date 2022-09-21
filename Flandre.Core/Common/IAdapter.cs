namespace Flandre.Core.Common;

public interface IAdapter<out TBot> : IModule where TBot : IBot
{
    public Task Start();
    public Task Stop();
    public IEnumerable<TBot> GetBots();
}

[AttributeUsage(AttributeTargets.Class)]
public class AdapterAttribute : Attribute
{
    public string Platform { get; init; } = "";
}