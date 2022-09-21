using Flandre.Core.Models;

namespace Flandre.Core.Messaging;

#pragma warning disable CS8618

public class Message
{
    public DateTime Time { get; init; } = DateTime.Now;
    public MessageSourceType SourceType { get; init; }
    public string MessageId { get; init; } = "";
    public string GuildId { get; init; } = "";
    public string ChannelId { get; init; } = "";
    public User Sender { get; init; }
    public MessageContent Content { get; init; }

    public string GetText()
    {
        return Content.GetText();
    }
}

public enum MessageSourceType
{
    Channel,
    Private
}