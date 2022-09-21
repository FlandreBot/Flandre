namespace Flandre.Core.Models;

public class GuildMember : User
{
    public IReadOnlySet<string> Roles { get; init; } = new HashSet<string>();
}