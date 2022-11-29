#pragma warning disable CS1591

namespace Flandre.Core.Common;

/// <summary>
/// 日志等级，与 Microsoft.Extension.Logging.LogLevel 兼容。
/// </summary>
public enum BotLogLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5,
    None = 6
}