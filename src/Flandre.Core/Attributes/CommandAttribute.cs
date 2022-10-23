using Flandre.Core.Utils;

namespace Flandre.Core.Attributes;

/// <summary>
/// 指令定义
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : Attribute
{
    /// <summary>
    /// 指令名称
    /// </summary>
    public string Command { get; }

    /// <summary>
    /// 参数定义
    /// </summary>
    public List<ParameterInfo> Parameters { get; } = new();

    /// <summary>
    /// 根据格式定义构造指令
    /// </summary>
    /// <param name="pattern">指令格式定义</param>
    public CommandAttribute(string pattern)
    {
        var parser = new StringParser(pattern);
        Command = string.Join('.', parser.Read(' ').Split('.',
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
        parser.SkipSpaces();

        var isNotRequiredParamAdded = false;

        while (!parser.IsEnd())
        {
            var bracket = parser.Current;
            if (!"<[".Contains(bracket))
            {
                parser.Read(' ');
                parser.SkipSpaces();
                continue;
            }

            var info = CommandUtils.ParseParameterSection(bracket switch
            {
                '<' => parser.Read('>', true),
                '[' => parser.Read(']', true),
                _ => parser.ReadToEnd()
            });

            if (info.IsRequired)
            {
                if (isNotRequiredParamAdded)
                    FlandreApp.Logger.Warning($"在指令 {Command} 的定义中，必选参数应位于可选参数之前，否则将造成难以预料的错误！");
            }
            else
            {
                isNotRequiredParamAdded = true;
            }

            Parameters.Add(info);

            FlandreApp.Logger.Debug(Parameters.Count.ToString());
            parser.SkipSpaces();
        }
    }
}

/// <summary>
/// 指令参数信息
/// </summary>
public class ParameterInfo
{
    internal ParameterInfo()
    {
    }

    /// <summary>
    /// 参数名称
    /// </summary>
    public string Name { get; internal set; } = "";

    /// <summary>
    /// 参数类型
    /// </summary>
    public string Type { get; internal set; } = "bool";

    /// <summary>
    /// 参数是否为必须，由括号类型推断
    /// </summary>
    public bool IsRequired { get; internal set; }


    /// <summary>
    /// 参数默认值
    /// </summary>
    public object DefaultValue { get; internal set; } = "";
}