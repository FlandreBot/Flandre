namespace Flandre.Framework.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class OptionAttribute : Attribute
{
    public string Name { get; }

    public OptionAttribute(string name) => Name = name;
}