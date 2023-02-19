using Flandre.Core.Models;

namespace Flandre.Framework.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : Attribute
{
    public CommandAttribute(string path, params string[] alias)
    {
        Path = path;
        Alias = alias;
    }

    public string Path { get; }

    public string[] Alias { get; }

    /// <summary>
    /// Default <see cref="UserRole.Member"/>
    /// </summary>
    public UserRole Role { get; init; } = UserRole.Member;

    /// <summary>
    /// Default <see langword="true"/>
    /// </summary>
    public bool IgnoreCase { get; init; } = true;
}