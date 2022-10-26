using Flandre.Core.Common;

namespace Flandre.Core.Events.App;

/// <summary>
/// 指令解析后
/// </summary>
public class AppCommandParsedEvent : BaseEvent
{
    /// <summary>
    /// 解析完毕的参数
    /// </summary>
    public ParsedArgs ParsedArgs { get; }

    /// <summary>
    /// 解析失败的提示。如果解析成功，此项为 <c>null</c>。
    /// </summary>
    public string? Error { get; }

    internal AppCommandParsedEvent(ParsedArgs args, string? error)
    {
        ParsedArgs = args;
        Error = error;
    }
}