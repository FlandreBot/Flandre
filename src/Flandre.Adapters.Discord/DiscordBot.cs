using System.Net;
using Discord;
using Discord.Net.Rest;
using Discord.Net.WebSockets;
using Discord.WebSocket;
using Flandre.Core.Common;
using Flandre.Core.Events;
using Flandre.Core.Messaging;

namespace Flandre.Adapters.Discord;

/// <summary>
/// Discord Bot
/// </summary>
public sealed partial class DiscordBot : Bot
{
    /// <inheritdoc cref="Bot.Platform"/>
    public override string Platform => "discord";

    /// <inheritdoc cref="Bot.SelfId"/>
    public override string SelfId { get; }

    /// <summary>
    /// Discord Client
    /// </summary>
    /// <seealso cref="global::Discord.IDiscordClient"/>
    public DiscordSocketClient Internal { get; }

    private readonly DiscordBotConfig _config;

    /// <inheritdoc/>
    public override event BotEventHandler<BotMessageReceivedEvent>? OnMessageReceived;

    /// <inheritdoc/>
    public override event BotEventHandler<BotGuildInvitedEvent>? OnGuildInvited;

    /// <inheritdoc/>
    public override event BotEventHandler<BotGuildJoinRequestedEvent>? OnGuildJoinRequested;

    /// <inheritdoc/>
    public override event BotEventHandler<BotFriendRequestedEvent>? OnFriendRequested;

    internal DiscordBot(DiscordBotConfig config, string? proxy)
    {
        _config = config;
        Internal = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
            RestClientProvider = DefaultRestClientProvider.Create(proxy is not null),
            WebSocketProvider = DefaultWebSocketProvider.Create(proxy is null ? null : new WebProxy(proxy))
        });
        SelfId = config.SelfId;

        // See InternalEventHandlers.cs
        Internal.Log += InternalOnLog;
        Internal.MessageReceived += InternalOnMessageReceived;
    }

    /// <seealso cref="Bot.StartAsync"/>
    public override async Task StartAsync()
    {
        if (_config.Token is null)
            throw new DiscordException("Bot token cannot be null.");

        await Internal.LoginAsync(TokenType.Bot, _config.Token);
        await Internal.StartAsync();
    }

    /// <inheritdoc cref="Bot.SendChannelMessageAsync"/>
    public override async Task<string?> SendChannelMessageAsync(string channelId, MessageContent content,
        string? guildId = null)
    {
        if (!InternalUtils.CheckIsValidId(channelId, out var parsed, this, "channel"))
            return null;

        var channel = await Internal.GetChannelAsync(parsed);
        if (channel is not SocketTextChannel textChannel)
            return null;

        var msg = await textChannel.SendMessageAsync(content.GetText());
        return msg?.Id.ToString();
    }

    /// <inheritdoc cref="Bot.SendPrivateMessageAsync"/>
    public override async Task<string?> SendPrivateMessageAsync(string userId, MessageContent content)
    {
        if (!InternalUtils.CheckIsValidId(userId, out var parsed, this, "user"))
            return null;

        var user = await Internal.GetUserAsync(parsed);
        var msg = await user.SendMessageAsync(content.GetText());
        return msg?.Id.ToString();
    }
}
