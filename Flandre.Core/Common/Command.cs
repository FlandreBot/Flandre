using System.Reflection;
using Flandre.Core.Attributes;

namespace Flandre.Core.Common;

/// <summary>
/// 指令
/// </summary>
public class Command
{
    /// <summary>
    /// 指令信息
    /// </summary>
    public CommandAttribute Info { get; }

    /// <summary>
    /// 指令依赖方法
    /// </summary>
    public MethodInfo InnerMethod { get; }

    /// <summary>
    /// 指令选项
    /// </summary>
    public List<OptionAttribute> Options { get; }

    internal Command(CommandAttribute info, MethodInfo innerMethod, List<OptionAttribute> options)
    {
        Info = info;
        InnerMethod = innerMethod;
        Options = options;
    }
}