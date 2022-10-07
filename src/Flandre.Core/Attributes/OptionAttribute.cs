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
    /// 短名称
    /// </summary>
    public char? ShortName { get; }

    /// <summary>
    /// 选项别名
    /// </summary>
    public string? Alias { get; }

    /// <summary>
    /// 选项类型
    /// </summary>
    internal string Type { get; }

    /// <summary>
    /// 选项默认值
    /// </summary>
    internal object DefaultValue { get; }

    /// <summary>
    /// 注册指令选项
    /// </summary>
    /// <param name="name">选项名称</param>
    /// <param name="pattern">选项格式</param>
    public OptionAttribute(string name, string? pattern = null)
    {
        Name = name.Trim();
        Type = "bool";
        DefaultValue = false;

        if (pattern == null) return;

        var parser = new StringParser(pattern);
        if (parser.SkipSpaces().IsEnd()) return;

        var first = parser.Read(' ');
        ParameterInfo info;

        if (first.StartsWith("--"))
        {
            Alias = first.TrimStart('-');
            if (parser.SkipSpaces().IsEnd()) return;
            info = CommandUtils.ParseParameterSection(parser.ReadToEnd(), cmdName: Name);
        }
        else if (first.StartsWith('-'))
        {
            var trimmed = first.TrimStart('-');
            ShortName = trimmed.Length > 0 ? trimmed[0] : null;
            if (parser.SkipSpaces().IsEnd()) return;
            info = CommandUtils.ParseParameterSection(parser.ReadToEnd(), cmdName: Name);
        }
        else
        {
            info = CommandUtils.ParseParameterSection(first, cmdName: Name);
        }

        Type = info.Type;
        DefaultValue = info.DefaultValue;
    }
}