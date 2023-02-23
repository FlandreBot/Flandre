namespace Flandre.Core.Models;

/// <summary>
/// 用户信息
/// </summary>
public class User
{
    /// <summary>
    /// 用户名称
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// 用户昵称
    /// </summary>
    public string? Nickname { get; init; }

    /// <summary>
    /// 用户 ID
    /// </summary>
    public string UserId { get; init; } = "";

    /// <summary>
    /// 用户 ID
    /// </summary>
    [Obsolete("User.Id 属性已弃用，请使用 User.UserId 替代。")]
    public string Id => UserId;

    /// <summary>
    /// 用户头像 URL
    /// </summary>
    public string? AvatarUrl { get; init; }
}
