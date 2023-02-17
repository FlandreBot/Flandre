namespace Flandre.Framework.Types;

/// <summary>
/// 命令类型
/// </summary>
public enum CmdType { 
    /// <summary>
    /// 普通命令
    /// </summary>
    Default = 0,
    /// <summary>
    /// 前缀命令
    /// </summary>
    /// <remarks>
    /// 表示这个命令只需要前缀匹配而无需空格
    /// </remarks>
    Prefix = 1,
}
