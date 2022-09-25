using System.Collections;

namespace Flandre.Core.Common;

/// <summary>
/// 指令选项
/// </summary>
public class OptionManager : IEnumerable<KeyValuePair<string, object>>
{
    internal readonly Dictionary<string, object> OptionsDict = new();

    /// <summary>
    /// 获取匹配类型的选项
    /// </summary>
    /// <param name="key">选项名称</param>
    /// <typeparam name="T">返回类型</typeparam>
    /// <returns>若未提供该选项，或类型错误则返回类型默认值</returns>
    public T? GetOrDefault<T>(string key)
    {
        var value = OptionsDict.GetValueOrDefault(key);
        return value is not null ? (T)value : default;
    }

    /// <summary>
    /// 获取 Enumerator
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        return OptionsDict.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}