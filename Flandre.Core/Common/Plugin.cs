using System.Reflection;
using Flandre.Core.Attributes;
using Flandre.Core.Messaging;
using Flandre.Core.Utils;

namespace Flandre.Core.Common;

/// <summary>
///     模块基类
/// </summary>
public abstract class Plugin : IModule
{
    /// <summary>
    ///     插件 Logger，默认使用插件名称
    /// </summary>
    public Logger Logger { get; }

    /// <summary>
    ///     插件的指令
    /// </summary>
    public List<Command> Commands { get; } = new();

    /// <summary>
    ///     插件信息
    /// </summary>
    public PluginAttribute PluginInfo { get; }

    /// <summary>
    ///     插件基类构造函数
    /// </summary>
    public Plugin()
    {
        var type = GetType();
        PluginInfo = type.GetCustomAttribute<PluginAttribute>() ?? new PluginAttribute(type.Name);
        Logger = new Logger(PluginInfo.Name);

        foreach (var method in type.GetMethods())
        {
            var attr = method.GetCustomAttribute<CommandAttribute>();
            if (attr is not null)
            {
                var options = new List<OptionAttribute>();

                foreach (var optionAttr in method.GetCustomAttributes<OptionAttribute>())
                    options.Add(optionAttr);

                Commands.Add(new Command(attr, method, options));
            }
        }
    }

    internal MessageContent? OnCommandParsing(Context ctx)
    {
        var commandStr = ctx.Message.GetText();
        if (string.IsNullOrWhiteSpace(commandStr)) return null;

        foreach (var command in Commands)
        {
            var basePattern = ctx.App.Config.CommandPrefix +
                              (PluginInfo.BaseCommand + ' ' +
                               command.Info.Command).TrimStart();

            var startsWithFlag = commandStr.StartsWith(basePattern);

            if (!startsWithFlag)
                continue;

            if (startsWithFlag)
                commandStr = commandStr[basePattern.Length..].Trim();

            var (parsedArgs, errorMsg) = ParseCommand(command, commandStr);
            if (errorMsg != null)
                return errorMsg;

            var result = command.InnerMethod.Invoke(
                this, new object[] { ctx, parsedArgs }[..command.InnerMethod.GetParameters().Length]);
            var content = result as MessageContent ?? (result as Task<MessageContent>)?.Result ?? null;
            return content;
        }

        return null;
    }

    internal (ParsedArgs, string?) ParseCommand(Command command, string source)
    {
        var args = new ParsedArgs();
        var parser = new StringParser(source);

        var argIndex = 0;
        var optionIndex = 0;

        while (!parser.IsEnd())
        {
            if (parser.Peek(' ').StartsWith('-'))
            {
                // option
                optionIndex++;
            }
            else
            {
                // argument
                if (argIndex >= command.Info.Parameters.Count)
                    return (args, "参数过多，请检查指令格式。");

                var param = command.Info.Parameters[argIndex];

                if (param.Type == "string")
                {
                    var quote = parser.Peek(1);
                    args.Arguments.ArgumentList.Add(
                        new KeyValuePair<string, object>(param.Name, quote switch
                        {
                            "\"" => parser.Read('\"', 1),
                            "\'" => parser.Read('\'', 1),
                            _ => parser.Read(' ')
                        }));
                }
                else
                {
                    if (CommandUtils.TryParseType(parser.Read(' '),
                            param.Type, out var result, false))
                        args.Arguments.ArgumentList.Add(new KeyValuePair<string, object>(param.Name, result));
                    else return (args, $"参数 {param.Name} 类型错误，应为 {param.Type}。");
                }

                param.Provided = true;
                argIndex++;
            }

            parser.SkipSpaces();
        }

        // 参数默认值
        foreach (var param in command.Info.Parameters)
        {
            if (param.IsRequired && !param.Provided)
                return (args, $"参数 {param.Name} 缺失。");
            if (param.IsRequired || param.Provided) continue;
            args.Arguments.ArgumentList.Add(new KeyValuePair<string, object>(param.Name, param.DefaultValue));
        }

        return (args, null);
    }

    /// <summary>
    ///     处理消息事件
    /// </summary>
    /// <param name="ctx">消息上下文</param>
    public virtual void OnMessageReceived(Context ctx)
    {
    }
}