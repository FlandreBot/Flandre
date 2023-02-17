using Flandre.Framework.Types;

namespace Flandre.Framework.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class CmdAttribute : Attribute
{
    public CmdAttribute(string name, params string[] alias)
    {
        Name = name;
        Alias = alias;
    }

    public string Name { get; }

    public string[] Alias { get; }

    public string Father { get; init; } = "";

    /// <summary>
    /// Default <see cref="RoleType.Member"/>
    /// </summary>
    public RoleType Permission { get; init; } = RoleType.Member;

    /// <summary>
    /// Default <see cref="CmdType.Default"/>
    /// </summary>
    public CmdType CommandType { get; init; } = CmdType.Default;

    /// <summary>
    /// Default <see cref="Types.ParameterType.Default"/>
    /// </summary>
    public ParameterType ParameterType { get; init; } = ParameterType.Default;

    /// <summary>
    /// Default <see langword="true"/>
    /// </summary>
    public bool IgnoreCase { get; init; } = true;
}
