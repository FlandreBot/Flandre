using Flandre.Core.Common;

namespace Flandre.Adapters.Discord;

internal static class InternalUtils
{
    internal static bool CheckIsValidId(string id, out ulong parsed, Bot bot, string idType)
    {
        var result = ulong.TryParse(id, out parsed);
        if (!result)
            bot.Log(BotLogLevel.Warning,
                $"Invalid {idType} id passed to Discord API. Must be parsable to uint64. Ignoring.");
        return result;
    }
}
