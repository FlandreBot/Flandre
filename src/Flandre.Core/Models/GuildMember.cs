namespace Flandre.Core.Models;

/// <summary>
/// Guild 成员
/// </summary>
public class GuildMember : User
{
    /// <summary>
    /// 成员角色
    /// </summary>
    public List<string> Roles { get; init; } = new();
}