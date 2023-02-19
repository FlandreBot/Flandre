namespace Flandre.Framework.Common;

public interface ICommandParameter
{
    public string Name { get; }

    public Type Type { get; }

    public object? DefaultValue { get; }

    public string? Description { get; init; }
}
