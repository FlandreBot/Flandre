namespace Flandre.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class PluginAttribute : Attribute
{
    public string Name { get; }

    public string? BaseCommand { get; set; }

    public PluginAttribute(string name)
    {
        Name = name;
    }
}