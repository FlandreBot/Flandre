namespace Flandre.Framework.Common;

public sealed class CommandParameter
{
    public string Name { get; }

    public Type Type { get; }

    public object? DefaultValue { get; }

    public bool IsRequired => DefaultValue is null;

    internal CommandParameter(string name, Type type, object? defaultValue = null)
    {
        Name = name;
        Type = type;
        DefaultValue = defaultValue;
    }

    internal CommandParameter(string name, Type type, string? defaultValue = null)
    {
        Name = name;
        Type = type;
        DefaultValue = defaultValue;
    }
}