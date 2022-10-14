using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using Flandre.Adapters.OneBot.Models;
using Flandre.Core.Common;
using Flandre.Core.Events.Bot;
using Flandre.Core.Messaging;
using Flandre.Core.Models;
using Flandre.Core.Utils;
using Websocket.Client;
using Websocket.Client.Exceptions;

namespace Flandre.Adapters.OneBot;

public class OneBotWebSocketBot : OneBotBot
{
    private readonly WebsocketClient _wsClient;
    private bool _clientStopped;

    private readonly Logger _logger;

    private readonly OneBotBotConfig _config;

    private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonElement>> _apiTasks = new();

    internal OneBotWebSocketBot(OneBotBotConfig config, Logger logger)
    {
        _config = config;
        _logger = logger;

        _wsClient = new WebsocketClient(new Uri(config.Endpoint!))
        {
            ReconnectTimeout = TimeSpan.FromSeconds(_config.WebSocketReconnectTimeout),
            ErrorReconnectTimeout = TimeSpan.FromSeconds(_config.WebSocketReconnectTimeout)
        };

        _wsClient.MessageReceived.Subscribe(OnApiMessage);

        _wsClient.DisconnectionHappened.Subscribe(_ =>
        {
            if (_clientStopped) return;
            _logger.Warning($"WebSocket 连接丢失，将在 {_config.WebSocketReconnectTimeout} 秒后尝试重连...");
            foreach (var tcs in _apiTasks.Values)
                tcs.SetException(new WebsocketException("WebSocket 连接丢失。"));
            _apiTasks.Clear();
        });

        _wsClient.ReconnectionHappened.Subscribe(_ => { _logger.Success("成功连接至 WebSocket 服务器。"); });
    }

    #region WebSocket 交互

    protected override Task<JsonElement> SendApiRequest(string action, object? @params = null)
    {
        var tcs = new TaskCompletionSource<JsonElement>();
        var echo = Guid.NewGuid().ToString();

        _apiTasks.TryAdd(echo, tcs);
        _wsClient.Send(JsonSerializer.Serialize(new { action, @params, echo }));

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
                    OnApiMessageEvent(json.Deserialize<OneBotApiMessageEvent>()!);
                    break;

                case "request":
                    var requestEvent = json.Deserialize<OneBotApiRequestEvent>()!;
                    if (requestEvent.RequestType == "group")
                    {
                        if (requestEvent.SubType == "invite")
                            OnGuildInvited?.Invoke(this, new BotGuildInvitedEvent(
                                    requestEvent.GroupId?.ToString()!, requestEvent.GroupId?.ToString()!,
                                    requestEvent.UserId.ToString(), requestEvent.UserId.ToString(), true)
                                { EventMessage = requestEvent.Flag });
                        else if (requestEvent.SubType == "add")
                            OnGuildRequested?.Invoke(this, new BotGuildRequestedEvent(
                                    requestEvent.GroupId?.ToString()!, requestEvent.GroupId?.ToString()!,
                                    requestEvent.UserId.ToString(), requestEvent.UserId.ToString(),
                                    requestEvent.Comment)
                                { EventMessage = requestEvent.Flag });
                    }
                    else if (requestEvent.RequestType == "friend")
                    {
                        OnFriendRequested?.Invoke(this, new BotFriendRequestedEvent(
                                requestEvent.UserId.ToString(), requestEvent.UserId.ToString(), requestEvent.Comment)
                            { EventMessage = requestEvent.Flag });
                    }

                    break;
            }
        }
        else
        {
            // 请求响应数据
            var resp = json.Deserialize<OneBotApiResponse>()!;
            if (!_apiTasks.TryGetValue(resp.Echo, out var tcs)) return;

            if (resp.Status == "failed")
                tcs.SetException(new OneBotApiException(resp.Msg ?? resp.Wording ?? $"调用 API 失败。({resp.RetCode})"));
            else
                tcs.SetResult(resp.Data);

            _apiTasks.TryRemove(resp.Echo, out _);
        }
    }

    private void OnApiMessageEvent(OneBotApiMessageEvent e)
    {
        OnMessageReceived?.Invoke(this, new BotMessageReceivedEvent(new Message
        {
            Time = DateTimeOffset.FromUnixTimeSeconds(e.Time).DateTime,
            SourceType = e.MessageType == "group" ? MessageSourceType.Channel : MessageSourceType.Private,
            MessageId = e.MessageId.ToString(),
            GuildId = e.GroupId?.ToString(),
            ChannelId = e.GroupId?.ToString(),
            Sender = new User
            {
                Name = e.Sender.Nickname,
                Nickname = e.Sender.Card,
                Id = e.UserId.ToString(),
                AvatarUrl = OneBotUtils.GetUserAvatar(e.UserId)
            },
            Content = CqCodeParser.ParseCqMessage(e.RawMessage)
        }));
    }

    #endregion

    public override async Task Start()
    {
        await _wsClient.Start();
        _clientStopped = false;
    }

    public override async Task Stop()
    {
        _clientStopped = true;
        await _wsClient.Stop(WebSocketCloseStatus.Empty, "");
        _wsClient.Dispose();
        _apiTasks.Clear();
    }

    public override event IBot.BotEventHandler<BotMessageReceivedEvent>? OnMessageReceived;
    public override event IBot.BotEventHandler<BotGuildInvitedEvent>? OnGuildInvited;
    public override event IBot.BotEventHandler<BotGuildRequestedEvent>? OnGuildRequested;
    public override event IBot.BotEventHandler<BotFriendRequestedEvent>? OnFriendRequested;
}