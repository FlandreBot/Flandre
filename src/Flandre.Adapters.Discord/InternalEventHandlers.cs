using Discord;
using Discord.WebSocket;
using Flandre.Core.Common;
using Flandre.Core.Events;
using Flandre.Core.Messaging;
using Flandre.Core.Models;

namespace Flandre.Adapters.Discord;

public sealed partial class DiscordBot
{
    private Task InternalOnLog(LogMessage log)
    {
        var logLevel = log.Severity switch
        {
            LogSeverity.Critical => BotLogLevel.Critical,
            LogSeverity.Error => BotLogLevel.Error,
            LogSeverity.Warning => BotLogLevel.Warning,
            LogSeverity.Info => BotLogLevel.Information,
            LogSeverity.Verbose => BotLogLevel.Trace,
            LogSeverity.Debug => BotLogLevel.Debug,

            _ => BotLogLevel.Debug
        };

        Log(logLevel, log.Message);
        if (log.Exception is { } ex)
            Log(logLevel, ex.ToString());

        return Task.CompletedTask;
    }

    private Task InternalOnMessageReceived(SocketMessage message)
    {
        MessageReceived?.Invoke(this, new BotMessageReceivedEvent(new Message
        {
            Time = message.Timestamp.DateTime,
            Platform = Platform,
            Environment = message.Channel is SocketDMChannel ? MessageEnvironment.Private : MessageEnvironment.Channel,
            MessageId = message.Id.ToString(),
            GuildId = message.Author is IGuildUser gu ? gu.Guild.Id.ToString() : null,
            ChannelId = message.Channel.Id.ToString(),
            Sender = new User
            {
                Name = message.Author.Username,
                Nickname = message.Author.Username,
                UserId = message.Author.Id.ToString(),
                AvatarUrl = message.Author.GetAvatarUrl()
            },
            Content = message.Content
        }));

        return Task.CompletedTask;
    }
}
