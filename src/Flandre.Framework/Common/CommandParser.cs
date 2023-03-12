using System.Collections;
using Flandre.Core.Utils;
using Flandre.Framework.Services;

namespace Flandre.Framework.Common;

internal static class CommandParser
{
    internal sealed class CommandParseResult
    {
        public List<object?> ParsedArguments { get; } = new();
        public Dictionary<string, object?> ParsedOptions { get; } = new();

        public string? ErrorText { get; internal set; }
    }

    /// <summary>
    /// 解析指令文本的参数及选项部分
    /// </summary>
    /// <param name="command">解析目标指令</param>
    /// <param name="parser">包含余下内容的 <see cref="StringParser"/></param>
    /// <param name="service"></param>
    /// <param name="result">消息解析内容</param>
    /// <returns>解析是否成功完成</returns>
    public static bool TryParse(this Command command, StringParser parser, CommandService service,
        out CommandParseResult result)
    {
        result = new CommandParseResult();

        var argIndex = 0;
        var providedArgs = new List<string>();

        while (!parser.IsEnd)
        {
            var peek = parser.SkipWhiteSpaces().PeekToWhiteSpace();

            // option (full)
            if (peek.StartsWith("--", StringComparison.OrdinalIgnoreCase))
            {
                var optName = parser.ReadToWhiteSpace().TrimStart('-');

                // 例: `--no-check` 将名为 `check` 的选项设置为 false
                // 如果者类型不是 bool，无事发生
                var optNo = false;

                if (optName.Length > 3 && optName.StartsWith("no-", StringComparison.OrdinalIgnoreCase))
                {
                    optName = optName[3..];
                    optNo = true;
                }

                var option = command.Options.FirstOrDefault(opt => opt.Name == optName);
                if (option is null)
                {
                    result.ErrorText = $"未知选项：{optName}";
                    return false;
                }

                if (option.Type == typeof(bool))
                {
                    result.ParsedOptions[option.Name] = !optNo;
                }
                else
                {
                    if (service.TryParseArgumentValue(option.Type,
                            parser.SkipWhiteSpaces().ReadToWhiteSpace(), out var obj))
                        result.ParsedOptions[option.Name] = obj;
                    else
                        return TypeNotMatch(result, option, service);
                }
            }
            else if (peek.StartsWith('-')) // option (short)
            {
                var opts = parser.ReadToWhiteSpace().TrimStart('-');

                parser.SkipWhiteSpaces();

                // 逐字符读取短选项，最后一个如果是非 bool 选项就读取一个参数给它，前面的全部赋值为 true
                for (var i = 0; i < opts.Length; i++)
                {
                    var optName = opts[i];
                    var option = command.Options.FirstOrDefault(opt => opt.HasShortName && opt.ShortName == optName);
                    if (option is null)
                    {
                        result.ErrorText = $"未知选项：{optName}";
                        return false;
                    }

                    if (option.Type == typeof(bool))
                        result.ParsedOptions[option.Name] = true;
                    else
                    {
                        // 由于只能赋值给最后一个短选项，前面的必须为 bool
                        if (i < opts.Length - 1)
                            return TypeNotMatch(result, option, service);

                        var nextArg = parser.ReadToWhiteSpace();

                        if (option.Type == typeof(string))
                            result.ParsedOptions[option.Name] = parser.ReadQuoted();
                        else if (service.TryParseArgumentValue(option.Type, nextArg, out var obj))
                            result.ParsedOptions[option.Name] = obj;
                        else
                            return TypeNotMatch(result, option, service);
                    }
                }
            }
            else // argument
            {
                if (argIndex >= command.Parameters.Count)
                {
                    result.ErrorText = "参数过多，请检查指令格式。";
                    return false;
                }

                var param = command.Parameters[argIndex];

                if (param.IsParamArray)
                {
                    var elemType = param.Type.GetElementType()!;
                    var list = new ArrayList();

                    while (!parser.IsEnd &&
                           service.TryParseArgumentValue(elemType,
                               elemType == typeof(string) ? parser.PeekQuoted() : parser.PeekToWhiteSpace(),
                               out var obj))
                    {
                        list.Add(obj);
                        parser.ReadToWhiteSpace();
                        parser.SkipWhiteSpaces();
                    }

                    var arr = Array.CreateInstance(elemType, list.Count);
                    list.CopyTo(arr);
                    result.ParsedArguments.Add(arr);
                }
                else if (param.Type == typeof(string))
                    result.ParsedArguments.Add(parser.ReadQuoted());
                else if (service.TryParseArgumentValue(param.Type, parser.ReadToWhiteSpace(), out var obj))
                    result.ParsedArguments.Add(obj);
                else
                    return TypeNotMatch(result, param, service);

                providedArgs.Add(param.Name);
                ++argIndex;
            }
        }

        // 默认值
        // 由于禁止在必选参数前添加可选参数，可以简单地用索引
        foreach (var param in command.Parameters)
        {
            var provided = providedArgs.Contains(param.Name);
            if (param.IsRequired && !provided)
            {
                result.ErrorText = $"参数 {param.Name} 缺失。";
                return false;
            }

            if (param.IsRequired || provided)
                continue;
            result.ParsedArguments.Add(param.DefaultValue!);
        }

        foreach (var opt in command.Options)
            if (!result.ParsedOptions.ContainsKey(opt.Name))
                result.ParsedOptions[opt.Name] = opt.DefaultValue;

        return true;
    }

    private static bool TypeNotMatch(CommandParseResult res, CommandOption option, CommandService service)
    {
        res.ErrorText = $"选项 {option.Name} 类型错误，应提供一个{service.GetTypeFriendlyName(option.Type)}。";
        return false;
    }

    private static bool TypeNotMatch(CommandParseResult res, CommandParameter param, CommandService service)
    {
        res.ErrorText = $"参数 {param.Name} 类型错误，应提供一个{service.GetTypeFriendlyName(param.Type)}。";
        return false;
    }
}
