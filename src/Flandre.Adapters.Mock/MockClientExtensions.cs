using Flandre.Core.Messaging;

namespace Flandre.Adapters.Mock;

public static class MockClientExtensions
{
    public static MessageContent? SendMessageForReply(this MockClient client, string message)
    {
        return client.SendMessageForReplyAsync(message).GetAwaiter().GetResult();
    }

    public static MessageContent? SendMessageForReply(this MockClient client, string message, TimeSpan timeout)
    {
        return client.SendMessageForReplyAsync(message, timeout).GetAwaiter().GetResult();
    }
}
