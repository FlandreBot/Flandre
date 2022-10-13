using Flandre.Core;
using Flandre.Core.Messaging;

namespace Flandre.Adapters.OneBot.Utils;

public static class OneBotUtils
{
    public static string EscapeCqCode(string source)
    {
        return source
            .Replace("&", "&amp;")
            .Replace("[", "&#91;")
            .Replace("]", "&#93;")
            .Replace(",", "&#44;");
    }

    public static string UnescapeCqCode(string source)
    {
        return source
            .Replace("&amp;", "&")
            .Replace("&#91;", "[")
            .Replace("&#93;", "]")
            .Replace("&#44;", ",");
    }

    /// <summary>
    /// 将含有 CQ 码的消息解析为 <see cref="MessageContent"/>。
    /// </summary>
    /// <param name="message">含有 CQ 码的消息</param>
    public static MessageContent ParseCqMessage(string message)
    {
        throw new NotImplementedException();
    }

    public static string ToCqMessage(this MessageContent content)
    {
        throw new NotImplementedException();
    }

    public static string GetUserAvatar(string userId)
    {
        return $"http://q.qlogo.cn/headimg_dl?dst_uin={userId}&spec=640";
    }

    public static string GetUserAvatar(long userId)
    {
        return $"http://q.qlogo.cn/headimg_dl?dst_uin={userId}&spec=640";
    }
}

public static class FlandreAppExtensions
{
    public static FlandreApp UseOneBotAdapter(this FlandreApp app, OneBotAdapterConfig config)
    {
        return app.Use(new OneBotAdapter(config));
    }
}

public class OneBotApiException : Exception
{
    public OneBotApiException(string message) : base(message)
    {
    }
}