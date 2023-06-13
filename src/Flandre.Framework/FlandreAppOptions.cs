namespace Flandre.Framework;

/// <summary>
/// 应用配置
/// </summary>
public sealed class FlandreAppOptions
{
    /// <summary>
    /// 全局指令前缀
    /// </summary>
    public string CommandPrefix { get; set; } = string.Empty;

    /// <summary>
    /// 在用户调用指令时，不进行“指令未找到”提示
    /// </summary>
    public bool IgnoreUndefinedCommand { get; set; } = false;
}
