namespace Flandre.Core;

/// <summary>
/// 应用配置
/// </summary>
public class FlandreAppConfig
{
    /// <summary>
    /// 全局指令前缀
    /// </summary>
    public string CommandPrefix { get; set; } = "";

    /// <summary>
    /// 忽略未定义指令的调用。可用值为：
    /// <br/> no - （默认）不忽略，调用未定义指令时发出警告信息
    /// <br/> root - 忽略根指令（顶级指令）
    /// </summary>
    public string IgnoreUndefinedCommand { get; set; } = "no";
}