using Flandre.Core.Common;

namespace Flandre.Adapters.Discord;

/// <summary>
/// Discord Bot 配置
/// </summary>
public class DiscordBotConfig : BotConfig
{
    /// <summary>
    /// Bot Token
    /// </summary>
    public string? Token { get; set; }
}
