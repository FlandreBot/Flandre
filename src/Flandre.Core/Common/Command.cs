using System.Reflection;
using Flandre.Core.Attributes;
using Flandre.Core.Messaging;
using Flandre.Core.Utils;

namespace Flandre.Core.Common;

/// <summary>
/// 指令
/// </summary>
public class Command
{
    /// <summary>
    /// 指令信息
    /// </summary>
    public CommandAttribute CommandInfo { get; }

    /// <summary>
    /// 指令依赖方法
    /// </summary>
    public MethodInfo InnerMethod { get; }

    /// <summary>
    /// 指令选项
    /// </summary>
    public List<OptionAttribute> Options { get; }

    /// <summary>
    /// 指令快捷方式
    /// </summary>
    public List<ShortcutAttribute> Shortcuts { get; }

    /// <summary>
    /// 指令别名
    /// </summary>
    public List<AliasAttribute> Aliases { get; }

    private readonly Plugin _plugin;

    internal Command(Plugin plugin, CommandAttribute info, MethodInfo innerMethod,
        List<OptionAttribute> options, List<ShortcutAttribute> shortcuts, List<AliasAttribute> aliases)
    {
        _plugin = plugin;
        CommandInfo = info;
        InnerMethod = innerMethod;
        Options = options;
        Shortcuts = shortcuts;
        Aliases = aliases;
    }

    internal MessageContent? ParseCommand(MessageContext ctx, StringParser parser)
    {
        var args = new ParsedArgs();

        var argIndex = 0;
        var providedArgs = new List<string>();

        while (!parser.IsEnd())
        {
            var peek = parser.SkipSpaces().Peek(' ');

            if (peek.StartsWith("--"))
            {
                // option (full)
                var optName = parser.Read(' ').TrimStart('-');
                var optNo = false;

                if (optName.Length > 3 && optName.StartsWith("no-"))
                {
                    optName = optName[3..];
                    optNo = true;
                }

                var option = Options.FirstOrDefault(opt => opt.Alias == optName)
                             ?? Options.FirstOrDefault(opt => opt.Name == optName);
                if (option is null)
                    return $"未知选项：{optName}。";

                parser.SkipSpaces();

                switch (option.Type)
                {
                    case "bool":
                        args.Options.OptionsDict[option.Name] = !optNo;
                        break;

                    case "string":
                        args.Options.OptionsDict[option.Name] = parser.ReadQuoted();
                        break;

                    default:
                        if (CommandUtils.TryParseType(parser.Read(' '),
                                option.Type, out var result, false))
                            args.Options.OptionsDict[option.Name] = result;
                        else return $"选项 {option.Name} 类型错误，应为 {option.Type}。";
                        break;
                }
            }
            else if (peek.StartsWith('-'))
            {
                // option (short)
                var opts = parser.Read(' ').TrimStart('-');

                parser.SkipSpaces();

                for (var i = 0; i < opts.Length; i++)
                {
                    var optName = opts[i];
                    var option = Options.FirstOrDefault(opt => opt.ShortName == optName);
                    if (option is null)
                        return $"未知选项：{optName}。";

                    if (option.Type == "bool")
                    {
                        args.Options.OptionsDict[option.Name] = true;
                    }
                    else
                    {
                        if (i < opts.Length - 1)
                            return $"选项 {option.Name} 类型错误，应为 {option.Type}。";

                        if (option.Type == "string")
                            args.Options.OptionsDict[option.Name] = parser.ReadQuoted();
                        else if (CommandUtils.TryParseType(parser.Read(' '),
                                     option.Type, out var result, false))
                            args.Options.OptionsDict[option.Name] = result;
                        else return $"选项 {option.Name} 类型错误，应为 {option.Type}。";
                    }
                }
            }
            else
            {
                // argument
                if (argIndex >= CommandInfo.Parameters.Count)
                    return "参数过多，请检查指令格式。";

                var param = CommandInfo.Parameters[argIndex];

                if (param.Type == "string")
                {
                    args.Arguments.ArgumentList.Add(
                        new KeyValuePair<string, object>(param.Name, parser.ReadQuoted()));
                }
                else
                {
                    if (CommandUtils.TryParseType(parser.Read(' '),
                            param.Type, out var result, false))
                        args.Arguments.ArgumentList.Add(new KeyValuePair<string, object>(param.Name, result));
                    else return $"参数 {param.Name} 类型错误，应为 {param.Type}。";
                }

                providedArgs.Add(param.Name);
                argIndex++;
            }
        }

        // 默认值
        foreach (var param in CommandInfo.Parameters)
        {
            var provided = providedArgs.Contains(param.Name);
            if (param.IsRequired && !provided)
                return $"参数 {param.Name} 缺失。";
            if (param.IsRequired || provided) continue;
            args.Arguments.ArgumentList.Add(new KeyValuePair<string, object>(param.Name, param.DefaultValue));
        }

        foreach (var opt in Options)
            if (!args.Options.OptionsDict.ContainsKey(opt.Name))
                args.Options.OptionsDict[opt.Name] = opt.DefaultValue;

        try
        {
            var cmdResult = InnerMethod.Invoke(
                _plugin, new object[] { ctx, args }[..InnerMethod.GetParameters().Length]);
            var content = cmdResult as MessageContent ?? (cmdResult as Task<MessageContent>)?.Result ?? null;

            return content;
        }
        catch (TargetInvocationException te)
        {
            _plugin.Logger.Error(te.InnerException ?? te);
        }
        catch (AggregateException ae)
        {
            _plugin.Logger.Error(ae.InnerException ?? ae);
        }
        catch (Exception e)
        {
            _plugin.Logger.Error(e);
        }

        return null;
    }
}