using Flandre.Core.Common;

namespace Flandre.Adapters.OneBot;

/// <summary>
/// OneBot 通信协议
/// </summary>
public enum OneBotProtocol
{
    /// <summary>
    /// 正向 WebSocket
    /// </summary>
    WebSocket
}

/// <summary>
/// OneBot 平台 Bot 配置
/// </summary>
public class OneBotBotConfig : BotConfig
{
    /// <summary>
    /// 连接 OneBot 服务端使用的协议。
    /// </summary>
    public OneBotProtocol Protocol { get; set; } = OneBotProtocol.WebSocket;

    /// <summary>
    /// 和 OneBot 服务端通信时使用的终结点。
    /// </summary>
    public string? Endpoint { get; set; } = null;

    /// <summary>
    /// WebSocket 服务端重连等待事件，单位为秒。
    /// </summary>
    public int WebSocketReconnectTimeout { get; set; } = 10;
}
