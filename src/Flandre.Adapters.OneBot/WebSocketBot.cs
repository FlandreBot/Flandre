using System.Collections.Concurrent;
using System.Text.Json;
using Flandre.Adapters.OneBot.Models;
using Flandre.Core.Common;
using Flandre.Core.Events;
using Flandre.Core.Messaging;
using Flandre.Core.Models;
using Websocket.Client;
using Websocket.Client.Exceptions;

namespace Flandre.Adapters.OneBot;

public sealed class OneBotWebSocketBot : OneBotBot
{
    private readonly WebsocketClient _wsClient;
    private bool _clientStopped;

    private readonly OneBotBotConfig _config;

    private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonElement>> _apiTasks = new();

    public override event BotEventHandler<BotMessageReceivedEvent>? MessageReceived;
    public override event BotEventHandler<BotGuildInvitedEvent>? GuildInvited;
    public override event BotEventHandler<BotGuildJoinRequestedEvent>? GuildJoinRequested;
    public override event BotEventHandler<BotFriendRequestedEvent>? FriendRequested;

    internal OneBotWebSocketBot(OneBotBotConfig config) : base(config.SelfId)
    {
        _config = config;

        _wsClient = new WebsocketClient(new Uri(config.Endpoint!))
        {
            ReconnectTimeout = TimeSpan.FromSeconds(_config.WebSocketReconnectTimeout),
            ErrorReconnectTimeout = TimeSpan.FromSeconds(_config.WebSocketReconnectTimeout)
        };

        _wsClient.MessageReceived.Subscribe(OnApiMessage);

        _wsClient.DisconnectionHappened.Subscribe(_ =>
        {
            if (_clientStopped)
                return;
            Log(BotLogLevel.Warning,
                $"WebSocket connection lost, reconnecting in {_config.WebSocketReconnectTimeout}s...");
            foreach (var tcs in _apiTasks.Values)
                tcs.SetException(new WebsocketException("WebSocket connection lost."));
            _apiTasks.Clear();
        });

        _wsClient.ReconnectionHappened.Subscribe(_ =>
        {
            Log(BotLogLevel.Information,
                $"Successfully connected to WebSocket server {config.Endpoint}.");
        });
    }

    #region WebSocket 交互

    internal override Task<JsonElement> SendApiRequest(string action, object? @params = null)
    {
        var tcs = new TaskCompletionSource<JsonElement>();
        var echo = Guid.NewGuid().ToString();

        _apiTasks.TryAdd(echo, tcs);
        _wsClient.Send(JsonSerializer.Serialize(new
        {
            action,
            @params,
            echo
        }));

        return tcs.Task;
    }

    private void OnApiMessage(ResponseMessage msg)
    {
        var json = JsonDocument.Parse(msg.Text);
        if (json.RootElement.TryGetProperty("post_type", out var postType))
        {
            // 上报事件
            switch (postType.ToString())
            {
                case "message":
                    json.RootElement.TryGetProperty("message_type", out var messageType);
                    if (messageType.ToString() == "guild")
                        GuildBot.InvokeMessageEvent(json.Deserialize<OneBotApiGuildMessageEvent>()!);
                    else
                        OnApiMessageEvent(json.Deserialize<OneBotApiMessageEvent>()!);
                    break;

                case "request":
                    var requestEvent = json.Deserialize<OneBotApiRequestEvent>()!;
                    if (requestEvent.RequestType == "group")
                    {
                        if (requestEvent.SubType == "invite")
                            GuildInvited?.Invoke(this, new BotGuildInvitedEvent(
                                requestEvent.GroupId?.ToString()!, requestEvent.GroupId?.ToString()!,
                                requestEvent.UserId.ToString(), requestEvent.UserId.ToString(), true)
                            {
                                EventPayload = requestEvent.Flag
                            });
                        else if (requestEvent.SubType == "add")
                            GuildJoinRequested?.Invoke(this, new BotGuildJoinRequestedEvent(
                                requestEvent.GroupId?.ToString()!, requestEvent.GroupId?.ToString()!,
                                requestEvent.UserId.ToString(), requestEvent.UserId.ToString(),
                                requestEvent.Comment) { EventPayload = requestEvent.Flag });
                    }
                    else if (requestEvent.RequestType == "friend")
                    {
                        FriendRequested?.Invoke(this, new BotFriendRequestedEvent(
                            requestEvent.UserId.ToString(), requestEvent.UserId.ToString(), requestEvent.Comment)
                        {
                            EventPayload = requestEvent.Flag
                        });
                    }

                    break;
            }
        }
        else
        {
            // 请求响应数据
            var resp = json.Deserialize<OneBotApiResponse>()!;
            if (!_apiTasks.TryGetValue(resp.Echo, out var tcs))
                return;

            if (resp.Status == "failed")
                tcs.SetException(new OneBotApiException(resp.Msg ?? resp.Wording ?? $"调用 API 失败。({resp.RetCode})"));
            else
                tcs.SetResult(resp.Data);

            _apiTasks.TryRemove(resp.Echo, out _);
        }
    }

    private void OnApiMessageEvent(OneBotApiMessageEvent e)
    {
        MessageReceived?.Invoke(this,
            new BotMessageReceivedEvent(new Message
            {
                Time = DateTimeOffset.FromUnixTimeSeconds(e.Time).DateTime,
                Platform = Platform,
                Environment = e.MessageType == "group" ? MessageEnvironment.Channel : MessageEnvironment.Private,
                MessageId = e.MessageId.ToString(),
                GuildId = e.GroupId?.ToString(),
                ChannelId = e.GroupId?.ToString(),
                Sender = new User
                {
                    Name = e.Sender.Nickname,
                    Nickname = e.Sender.Card,
                    UserId = e.UserId.ToString(),
                    AvatarUrl = OneBotUtils.GetUserAvatar(e.UserId)
                },
                Content = CqCodeParser.ParseCqMessage(e.RawMessage)
            }));
    }

    #endregion

    public override Task StartAsync()
    {
        _ = _wsClient.Start();
        _clientStopped = false;
        return Task.CompletedTask;
    }

    public override Task StopAsync()
    {
        _clientStopped = true;
        _wsClient.Dispose();
        _apiTasks.Clear();
        return Task.CompletedTask;
    }
}
