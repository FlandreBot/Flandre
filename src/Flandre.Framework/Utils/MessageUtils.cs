using Flandre.Core.Messaging;

namespace Flandre.Framework.Utils;

internal static class MessageUtils
{
    internal static string GetUserMark(this MessageContext ctx)
    {
        return $"{ctx.Platform}:{ctx.GuildId}:{ctx.UserId}";
    }
}