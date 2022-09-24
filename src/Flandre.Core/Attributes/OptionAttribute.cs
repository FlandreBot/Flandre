using Flandre.Core.Utils;

namespace Flandre.Core.Attributes;

/// <summary>
/// 指令选项特性
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class OptionAttribute : Attribute
{
    /// <summary>
    /// 选项名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 选项别名
    /// </summary>
    public string? Alias { get; }

    /// <summary>
    /// 选项类型
    /// </summary>
    internal string Type { get; }

    /// <summary>
    /// 选项值是否为必选
    /// </summary>
    internal bool IsValueRequired { get; }

    /// <summary>
    /// 注册指令选项
    /// </summary>
    /// <param name="name">选项名称</param>
    /// <param name="pattern">选项格式</param>
    public OptionAttribute(string name, string? pattern = null)
    {
        Name = name;
        Type = "bool";

        if (pattern == null) return;

        var patterns = pattern.Split(' ',
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        string? nameDef = null, paramDef = null;

        if (patterns.Length == 1)
        {
            if (patterns[0].StartsWith('-'))
                nameDef = patterns[0];
            else paramDef = patterns[0];
        }
        else
        {
            nameDef = patterns[0];
            paramDef = patterns[1];
        }

        if (nameDef != null)
            Alias = nameDef.TrimStart('-');

        if (paramDef != null)
        {
            var info = CommandUtils.ParseParameterSection(paramDef);
            Type = info.Type;
            IsValueRequired = info.IsRequired;
        }
    }
}