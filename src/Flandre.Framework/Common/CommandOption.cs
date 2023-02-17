namespace Flandre.Framework.Common;

public sealed class CommandOption
{
    public string Name { get; }

    public char ShortName { get; }

    public bool HasShortName { get; }

    public Type Type { get; }

    public object DefaultValue { get; }

    public string Description { get; init; } = "";

    internal CommandOption(string name, char shortName, object defaultValue)
    {
        Name = name;
        ShortName = shortName;
        HasShortName = true;
        Type = defaultValue.GetType();
        DefaultValue = defaultValue;
    }

    internal CommandOption(string name, object defaultValue)
    {
        Name = name;
        HasShortName = false;
        Type = defaultValue.GetType();
        DefaultValue = defaultValue;
    }
}
