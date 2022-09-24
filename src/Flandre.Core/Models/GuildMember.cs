namespace Flandre.Core.Models;

/// <summary>
/// Guild 成员
/// </summary>
public class GuildMember : User
{
    public IReadOnlySet<string> Roles { get; init; } = new HashSet<string>();
}