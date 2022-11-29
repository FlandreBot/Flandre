using Flandre.Core.Common;
using Flandre.Core.Messaging;

namespace Flandre.Framework.Common;

public class MiddlewareContext : MessageContext
{
    public MessageContent? Response { get; set; }

    internal MiddlewareContext(Bot bot, Message message, MessageContent? resp)
        : base(bot, message)
    {
        Response = resp;
    }
}