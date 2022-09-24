namespace Flandre.Core.Common;

/// <summary>
/// 指令选项
/// </summary>
public class OptionManager
{
    private readonly Dictionary<string, object> _optionsDict = new();

    /// <summary>
    /// 获取选项值
    /// </summary>
    /// <param name="key">选项名称</param>
    /// <returns>若未提供该选项则返回 null</returns>
    public string? Get(string key)
    {
        return _optionsDict.GetValueOrDefault(key)?.ToString();
    }

    /// <summary>
    /// 获取匹配类型的选项
    /// </summary>
    /// <param name="key">选项名称</param>
    /// <typeparam name="T">返回类型</typeparam>
    /// <returns>若未提供该选项，或类型错误则返回类型默认值</returns>
    public T? Get<T>(string key)
    {
        var value = _optionsDict.GetValueOrDefault(key);
        return value is not null ? (T)value : default;
    }
}