namespace Flandre.Adapters.OneBot;

public static class OneBotUtils
{
    public static string EscapeCqCode(string source, bool escapeComma = false)
    {
        if (escapeComma)
            source = source.Replace(",", "&#44;");
        return source
            .Replace("&", "&amp;")
            .Replace("[", "&#91;")
            .Replace("]", "&#93;");
    }

    public static string UnescapeCqCode(string source, bool unescapeComma = false)
    {
        if (unescapeComma)
            source = source.Replace("&#44;", ",");
        return source
            .Replace("&amp;", "&")
            .Replace("&#91;", "[")
            .Replace("&#93;", "]");
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

public class OneBotApiException : Exception
{
    public OneBotApiException(string message) : base(message)
    {
    }
}
