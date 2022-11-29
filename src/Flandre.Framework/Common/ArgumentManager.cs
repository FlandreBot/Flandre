using System.Collections;

namespace Flandre.Framework.Common;

/// <summary>
/// 指令参数
/// </summary>
public class ArgumentManager : IEnumerable<KeyValuePair<string, object>>
{
    internal readonly List<KeyValuePair<string, object>> ArgumentList = new();

    /// <summary>
    /// 获取参数列表的 Enumerator
    /// </summary>
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        return ArgumentList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// 获取参数
    /// </summary>
    /// <param name="index">参数索引</param>
    public object this[int index] => ArgumentList[index].Value;

    /// <summary>
    /// 根据索引获取参数，值将被强制转换为 T 类型
    /// </summary>
    /// <param name="index">参数索引</param>
    /// <typeparam name="T">返回类型</typeparam>
    public T Get<T>(int index)
    {
        return (T)ArgumentList[index].Value;
    }

    /// <summary>
    /// 根据名称获取参数，值将被强制转换为 T 类型
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <typeparam name="T">返回类型</typeparam>
    public T Get<T>(string name)
    {
        return (T)ArgumentList.First(arg => arg.Key == name).Value;
    }
}