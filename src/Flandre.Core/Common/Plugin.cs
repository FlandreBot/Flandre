using System.Reflection;
using Flandre.Core.Attributes;
using Flandre.Core.Events.Bot;
using Flandre.Core.Messaging;
using Flandre.Core.Utils;

namespace Flandre.Core.Common;

/// <summary>
/// 模块基类
/// </summary>
public abstract class Plugin : IModule
{
    /// <summary>
    /// 插件 Logger，默认使用插件名称
    /// </summary>
    public Logger Logger { get; }

    /// <summary>
    /// 插件的指令
    /// </summary>
    public List<Command> Commands { get; } = new();

    /// <summary>
    /// 插件信息
    /// </summary>
    public PluginAttribute PluginInfo { get; }

    /// <summary>
    /// 插件基类构造函数
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

    internal MessageContent? OnCommandParsing(MessageContext ctx)
    {
        var commandStr = ctx.Message.GetText();
        if (string.IsNullOrWhiteSpace(commandStr)) return null;

        foreach (var command in Commands)
        {
            var basePattern = ctx.App.Config.CommandPrefix +
                              (PluginInfo.BaseCommand + ' ' +
                               command.CommandInfo.Command).TrimStart();

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
        var providedArgs = new List<string>();

        while (!parser.IsEnd())
        {
            var peek = parser.SkipSpaces().Peek(' ');

            if (peek.StartsWith('-'))
            {
                // option
                var optName = parser.Read(' ').TrimStart('-');
                var optNo = false;

                if (optName.Length > 3 && optName.StartsWith("no-"))
                {
                    optName = optName[3..];
                    optNo = true;
                }

                var option = command.Options.FirstOrDefault(opt => opt.Alias == optName)
                             ?? command.Options.FirstOrDefault(opt => opt.Name == optName);
                if (option is null)
                    return (args, $"未知选项：{optName}。");

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
                        else return (args, $"选项 {option.Name} 类型错误，应为 {option.Type}。");
                        break;
                }
            }
            else
            {
                // argument
                if (argIndex >= command.CommandInfo.Parameters.Count)
                    return (args, "参数过多，请检查指令格式。");

                var param = command.CommandInfo.Parameters[argIndex];

                if (param.Type == "string")
                {
                    var quote = parser.Peek(1);
                    args.Arguments.ArgumentList.Add(
                        new KeyValuePair<string, object>(param.Name, parser.ReadQuoted()));
                }
                else
                {
                    if (CommandUtils.TryParseType(parser.Read(' '),
                            param.Type, out var result, false))
                        args.Arguments.ArgumentList.Add(new KeyValuePair<string, object>(param.Name, result));
                    else return (args, $"参数 {param.Name} 类型错误，应为 {param.Type}。");
                }

                providedArgs.Add(param.Name);
                argIndex++;
            }
        }

        // 默认值
        foreach (var param in command.CommandInfo.Parameters)
        {
            var provided = providedArgs.Contains(param.Name);
            if (param.IsRequired && !provided)
                return (args, $"参数 {param.Name} 缺失。");
            if (param.IsRequired || provided) continue;
            args.Arguments.ArgumentList.Add(new KeyValuePair<string, object>(param.Name, param.DefaultValue));
        }

        foreach (var opt in command.Options)
            if (!args.Options.OptionsDict.ContainsKey(opt.Name))
                args.Options.OptionsDict[opt.Name] = opt.DefaultValue;

        return (args, null);
    }

    /// <summary>
    /// 处理消息事件
    /// </summary>
    /// <param name="ctx">消息上下文</param>
    public virtual void OnMessageReceived(MessageContext ctx)
    {
    }

    /// <summary>
    /// 收到拉群邀请
    /// </summary>
    /// <param name="ctx">当前上下文</param>
    /// <param name="e">拉群邀请事件</param>
    public virtual void OnGuildInvited(Context ctx, BotGuildInvitedEvent e)
    {
    }

    /// <summary>
    /// 收到入群申请
    /// </summary>
    /// <param name="ctx">当前上下文</param>
    /// <param name="e">入群申请事件</param>
    public virtual void OnGuildRequested(Context ctx, BotGuildRequestedEvent e)
    {
    }

    /// <summary>
    /// 收到好友申请
    /// </summary>
    /// <param name="ctx">当前上下文</param>
    /// <param name="e">好友申请事件</param>
    public virtual void OnFriendRequested(Context ctx, BotFriendRequestedEvent e)
    {
    }
}