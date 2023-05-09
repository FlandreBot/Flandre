namespace Flandre.Framework.Common;

/// <summary>
/// 指令解析结果
/// </summary>
public sealed class CommandParseResult
{
    /// <summary>
    /// 参数解析结果
    /// </summary>
    public List<object?> ParsedArguments { get; } = new();

    /// <summary>
    /// 选项解析结果（选项名-值）
    /// </summary>
    public Dictionary<string, object?> ParsedOptions { get; } = new();

    internal string? ErrorMessage { get; private set; }

    /// <summary>
    /// 设置为解析错误，并发送一条提示消息给用户
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <returns></returns>
    public CommandParseResult SetError(string message)
    {
        ErrorMessage = message;
        return this;
    }
}
