namespace Flandre.Framework.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class OptionAttribute : Attribute
{
    public char ShortName { get; init; }

}
