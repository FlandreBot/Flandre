namespace Flandre.Adapters.Discord;

/// <summary>
/// Discord 异常
/// </summary>
public sealed class DiscordException : Exception
{
    internal DiscordException(string message) : base(message) { }
}
