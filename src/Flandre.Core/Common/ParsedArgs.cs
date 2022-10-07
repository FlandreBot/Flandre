namespace Flandre.Core.Common;

/// <summary>
/// 解析后的指令参数/选项
/// </summary>
public class ParsedArgs
{
    /// <summary>
    /// 指令参数
    /// </summary>
    public ArgumentManager Arguments { get; } = new();

    /// <summary>
    /// 指令选项
    /// </summary>
    public OptionManager Options { get; } = new();

    /// <summary>
    /// 根据索引获取参数，值将被强制转换为 T 类型
    /// </summary>
    /// <param name="index">参数索引</param>
    /// <typeparam name="T">返回类型</typeparam>
    public T GetArgument<T>(int index)
    {
        return Arguments.Get<T>(index);
    }

    /// <summary>
    /// 根据名称获取参数，值将被强制转换为 T 类型
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <typeparam name="T">返回类型</typeparam>
    public T GetArgument<T>(string name)
    {
        return Arguments.Get<T>(name);
    }

    /// <summary>
    /// 根据名称获取选项，值将被强制转换为 T 类型
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <typeparam name="T">返回类型</typeparam>
    public T GetOption<T>(string name)
    {
        return Options.Get<T>(name);
    }
}