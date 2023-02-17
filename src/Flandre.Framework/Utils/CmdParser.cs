using Flandre.Core.Common;
using Flandre.Core.Messaging;
using Flandre.Framework.Attributes;
using Types;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Flandre.Framework.Common;
using System.Linq;

namespace Flandre.Framework.Utils;


/// <summary>
/// 处理命令
/// </summary>
public static class CmdParser
{
    internal static void Parse<T>(CommandNode root, Type pluginType)
    {
        var d = new Dictionary<string, CommandNode>()
        {
            [""] = root
        };
        foreach (var method in pluginType.GetMethods())
            // 没有标注是命令的
            if (method.GetCustomAttribute<CmdAttribute>() is { } cmdAttribute)
            {
                if (Get<T>(method, pluginType) is not { } c)
                    continue;
                d[cmdAttribute.Name] = c;
                if (!d.TryGetValue(cmdAttribute.Father, out var value))
                    value = d[cmdAttribute.Father] = new CommandNode(false);
                value.Subcommands.Add(cmdAttribute.Name, c);
            }
    }

    /// <summary>
    /// 尝试反射获取命令
    /// </summary>
    /// <returns></returns>
    internal static CommandNode? Get<T>(MethodInfo method, Type pluginType)
    {
        // 方法类型不对的
        if (!(method.ReturnType.IsAssignableFrom(typeof(T))
            || method.ReturnType.IsAssignableFrom(typeof(Task<T>))
            || method.ReturnType.IsAssignableFrom(typeof(ValueTask<T>))
            || method.ReturnType.IsAssignableFrom(typeof(T[]))
            || method.ReturnType.IsAssignableFrom(typeof(Task<T[]>))
            || method.ReturnType.IsAssignableFrom(typeof(ValueTask<T[]>))))
        {
            Console.Error.WriteLine($"警告: 命令方法\"[{method.ReflectedType?.FullName}]::{method.Name}()\"的返回类型不是{typeof(T)}, 将忽略这个命令！");
            return null;
        }

        var temp = method.GetParameters()
            .GroupBy(t => t.GetCustomAttribute<OptionAttribute>() is null)
            .ToDictionary(i => i.Key, i => new HashSet<ParameterInfo>(i));

        var parameters = temp[true].Select(t =>
            new CommandParameter(t.Name!, t.ParameterType, t.DefaultValue)
            {
                Description = t.GetCustomAttribute<DescriptionAttribute>()?.Description ?? ""
            }).ToList();
        var options = temp[false].Select(t =>
            new CommandOption(t.Name!, t.GetCustomAttribute<OptionAttribute>()!.ShortName, t.DefaultValue!)
            {
                Description = t.GetCustomAttribute<DescriptionAttribute>()?.Description ?? ""
            }).ToList();

        var node = new CommandNode(false);
        node.Command = new(node, pluginType, "")
        {
            Parameters = parameters,
            Options = options,
            IsObsoleted = method.GetCustomAttribute<ObsoleteAttribute>() is not null,
            Description = method.GetCustomAttribute<DescriptionAttribute>()?.Description ?? ""
        };


        return node;

        // Console.WriteLine($"发现Cmdlet: {cmdlet.Permission} {cmdlet.CommandType} {cmdlet.ReturnType} {(cmdlet.IgnoreCase ? "" : "*")}{cmdlet.Name}({string.Join(", ", cmdlet.Parameters.Select(i => $"{i.Type} {i.Name}{(i.HasDefault ? $" = {i.Default}" : "")}"))})");
    }
    /*
    /// <summary>
    /// 解析命令
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="group"></param>
    /// <param name="raw">原始字符串</param>
    /// <returns></returns>
    public static async Task<bool> Process(Bot bot, GroupMessageEvent group, Raw raw)
    {
        try
        {
            if (raw.SplitArgs.Length is 0 || raw.SplitArgs[0].Trim().Contains(' '))
                return false;

            MessageBuilder[]? result = null;

            if (BotResponse.Cmdlets.TryGetValue(CmdletType.Default, out var set))
                if (ParseCommand<MessageBuilder>(bot, group, raw, CmdletType.Default, set) is { } r)
                    result = await r;

            if (result is null && BotResponse.Cmdlets.TryGetValue(CmdletType.Prefix, out set))
                if (ParseCommand<MessageBuilder>(bot, group, raw, CmdletType.Prefix, set) is { } r)
                    result = await r;

            if (result is not null)
            {
                foreach (var messageBuilder in result)
                {
                    _ = await bot.SendGroupMessage(group.GroupUin, messageBuilder);
                }

                return true;
            }
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync(e.ToString());
        }

        return false;
    }

    /// <summary>
    /// 解析命令
    /// </summary>
    /// <param name="bot">机器人实例</param>
    /// <param name="group">群消息事件实例</param>
    /// <param name="raw">生字符串</param>
    /// <param name="type">命令类型</param>
    /// <param name="set">Cmdlet集</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">命令名不能为空</exception>
    /// <exception cref="InvalidOperationException">找不到合适的命令</exception>
    /// <exception cref="NotSupportedException">类型解析器不支持的类型</exception>
    private static Task<T[]>? ParseCommand<T>(
        Bot bot,
        GroupMessageEvent group,
        Raw raw,
        CmdletType type,
        HashSet<Record<CmdletAttribute>> set)
    {
        var cmd = raw.SplitArgs[0].Trim();

        Func<string?, string[], StringComparison, bool> matcher = type switch
        {
            CmdletType.Default => (i, o, s) => o.Any(t => string.Equals(i, t, s)),
            CmdletType.Prefix => (i, o, s) => o.Any(t =>
            {
                if (i?.StartsWith(t, s) is true)
                {
                    cmd = t;
                    return true;
                }

                return false;
            }),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

        var cmdSet = set.Where(c => !c.IsObsoleted)
            .Where(i => matcher(cmd, i.Attribute.Names,
                i.Attribute.IgnoreCase
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal))
            // 按参数数量从大到小排序
            .OrderBy(i => -i.Parameters.Length)
            .ToArray();

        foreach (var cmdlet in cmdSet)
        {
            string[] args;
            switch (type)
            {
                case CmdletType.Default:
                    args = raw.SplitArgs[1..];
                    break;
                case CmdletType.Prefix:
                    args = raw.SplitArgs;
                    args[0] = args[0][cmd.Length..];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }

            if (cmdlet.ParseArguments(bot, group, raw, args, out var parameters))
                return cmdlet.InvokeArrayAsync<T, CmdletAttribute>(bot, group, parameters);
        }

        // 找不到合适的Cmdlet重载
        return null;
    }
    */
}

