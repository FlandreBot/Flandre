using Flandre.Core.Utils;

namespace Flandre.Framework.Common;

/// <summary>
/// 指令解析器
/// </summary>
public interface ICommandParser
{
    /// <summary>
    /// 解析指令的参数、选项部分
    /// </summary>
    /// <param name="command">当前指令</param>
    /// <param name="parser">当前解析器</param>
    /// <returns>解析结果。如果解析失败，使用 <see cref="CommandParseResult.SetError(string)"/> 说明。</returns>
    CommandParseResult Parse(Command command, StringParser parser);
}
