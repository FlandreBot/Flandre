namespace Flandre.Framework.Common;

public sealed class CommandParameter : ICommandParameter
{
    public string Name { get; }

    public Type Type { get; }

    public object? DefaultValue { get; }

    public string? Description { get; init; }

    public bool IsRequired => DefaultValue is null;

    internal CommandParameter(string name, Type type, object? defaultValue)
    {
        Name = name;
        Type = type;
        DefaultValue = defaultValue;
    }
}
