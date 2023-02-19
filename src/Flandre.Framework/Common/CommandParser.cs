using System.Text;
using Flandre.Core.Utils;
using Flandre.Framework.Utils;

namespace Flandre.Framework.Common;

internal static class CommandParser
{
    internal sealed class CommandSplitResult
    {
        public List<string> Arguments { get; } = new();

        public Dictionary<string, string> Options { get; } = new();
    }

    internal sealed class CommandParseResult
    {
        public List<object> Arguments { get; } = new();

        public Dictionary<string, object> Options { get; } = new();

        public StringBuilder ErrorText { get; } = new();
    }

    public static List<string> SplitArgs(this StringParser parser)
    {
        var args = new List<string>();
        while (!parser.IsEnd)
        {
            args.Add(parser.ReadQuoted());
            parser.SkipWhiteSpaces();
        }
        return args;
    }

    public static bool TrySplit(this StringParser parser, out CommandSplitResult result)
    {
        result = new CommandSplitResult();
        string? waitForOption = null;
        foreach (var str in parser.SplitArgs())
        {
            if (str.StartsWith("--") || str.StartsWith('-'))
            {
                var s = str.TrimStart('-');
                // 以第一次出现的参数为准，第二次出现忽略
                if (!result.Options.ContainsKey(str))
                {
                    result.Options[str] = "";
                    waitForOption = str;
                }
            }
            else if (waitForOption is not null)
            {
                result.Options[waitForOption] = str;
                waitForOption = null;
            }
            else
                result.Arguments.Add(str);
        }

        return true;
    }

    /// <summary>
    /// 解析指令文本的参数及选项部分
    /// </summary>
    /// <param name="command">解析目标指令</param>
    /// <param name="parser">包含余下内容的 <see cref="StringParser"/></param>
    /// <param name="result">消息解析内容</param>
    /// <returns>解析是否成功完成</returns>
    public static bool TryParse(this Command command, StringParser parser, out CommandParseResult result)
    {
        result = new CommandParseResult();

        if (!parser.TrySplit(out var split))
            return false;

        if (split.Arguments.Count > command.Parameters.Count)
            result.ErrorText.AppendLine("参数过多，请检查指令格式。");

        if (split.Arguments.Count < command.Parameters.Count(t => t.IsRequired))
        {
            result.ErrorText.AppendLine("参数缺少，请检查指令格式。");
            return false;
        }

        for (var i = 0; i < command.Parameters.Count; ++i)
            // --- start parse --- //
            if (CommandUtils.TryParseValue(split.Arguments[i], command.Parameters[i].Type, out var obj))
                result.Arguments[i] = obj;
            else
            {
                result.ErrorText.Append("参数").AppendLine(TypeNotMatch(command.Parameters[i]));
                return false;
            }
        // ------- end parse --- //

        if (split.Options.Count > command.Options.Count)
            result.ErrorText.AppendLine("选项过多，请检查指令格式。");

        foreach (var option in command.Options)
            // --option
            if (split.Options.TryGetValue(option.Name, out var str))
            {
                if (option.Type == typeof(bool))
                    result.Options[option.Name] = RedundantOptionValue(str, result.ErrorText);
                // --- start parse --- //
                else if (CommandUtils.TryParseValue(str, option.Type, out var obj))
                    result.Options[option.Name] = obj;
                else
                {
                    result.ErrorText.Append("选项").AppendLine(TypeNotMatch(option));
                    return false;
                }
                // --- end parse --- //
                split.Options.Remove(str);
            }

            // --no-option
            else if (split.Options.TryGetValue("no-" + option.Name, out str))
            {
                if (option.Type == typeof(bool))
                    result.Options[option.Name] = !RedundantOptionValue(str, result.ErrorText);
                else
                {
                    result.ErrorText.Append("选项").AppendLine(TypeNotMatch(option));
                    return false;
                }
                split.Options.Remove(str);
            }

            // -o
            else if (option.HasShortName && split.Options.TryGetValue(option.ShortName.ToString(), out str))
            {
                if (option.Type == typeof(bool))
                    result.Options[option.Name] = RedundantOptionValue(str, result.ErrorText);
                // --- start parse --- //
                else if (CommandUtils.TryParseValue(str, option.Type, out var obj))
                    result.Options[option.Name] = obj;
                else
                {
                    result.ErrorText.Append("选项").AppendLine(TypeNotMatch(option));
                    return false;
                }
                // --- end parse --- //
                split.Options.Remove(str);
            }

        foreach (var option in split.Options)
            result.ErrorText.AppendLine($"未知选项 {option.Key}。");

        return true;
    }

    private static string TypeNotMatch(ICommandParameter parameter)
        => $" {parameter.Name} 类型错误，应提供一个{CommandUtils.GetTypeDescription(parameter.Type)}。";

    private static bool RedundantOptionValue(string value, StringBuilder warning)
    {
        if (value is not "")
            warning.AppendLine($"选项不应该提供参数 {value}。");
        return true;
    }
}
