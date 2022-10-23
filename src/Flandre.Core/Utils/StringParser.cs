namespace Flandre.Core.Utils;

/// <summary>
/// 字符串解析器
/// </summary>
public class StringParser
{
    private readonly string _str;
    private int _pos;

    /// <summary>
    /// 当前字符
    /// </summary>
    public char Current => _str[_pos];

    /// <summary>
    /// 使用字符串构造解析器
    /// </summary>
    public StringParser(string str)
    {
        _str = str;
    }

    /// <summary>
    /// 字符串是否解析完
    /// </summary>
    public bool IsEnd()
    {
        return _pos >= _str.Length;
    }

    /// <summary>
    /// 跳过指定长度
    /// </summary>
    public StringParser Skip(int length)
    {
        _pos += length;
        return this;
    }

    /// <summary>
    /// 跳到指定的字符位置
    /// </summary>
    /// <param name="terminator">终点字符</param>
    public StringParser Skip(char terminator)
    {
        _pos = FixIndex(_str.IndexOf(terminator, _pos));
        return this;
    }

    /// <summary>
    /// 跳过空格
    /// </summary>
    public StringParser SkipSpaces()
    {
        while (!IsEnd())
        {
            if (_str[_pos] != ' ') break;
            _pos++;
        }

        return this;
    }

    /// <summary>
    /// 读取字符串，但不移动解析器指针
    /// </summary>
    /// <param name="length">读取字符串的长度</param>
    public string Peek(int length)
    {
        return _str.Substring(_pos, length);
    }

    /// <summary>
    /// 读取字符串，但不移动解析器指针
    /// </summary>
    /// <param name="terminator">终点字符</param>
    public string Peek(char terminator)
    {
        var end = FixIndex(_str.IndexOf(terminator, _pos));
        return _str.Substring(_pos, end - _pos);
    }

    /// <summary>
    /// 读取字符串，且移动解析器指针
    /// </summary>
    /// <param name="terminator">终点字符，将解析器指针指向该字符</param>
    /// <param name="includeTerminator">同时读取终点字符，解析器指针指向下一字符</param>
    public string Read(char terminator, bool includeTerminator = false)
    {
        var end = FixIndex(_str.IndexOf(terminator, _pos));
        var value = _str.Substring(_pos, end - _pos);
        _pos = end;

        if (includeTerminator)
        {
            value += terminator;
            _pos++;
        }

        return value;
    }

    /// <summary>
    /// 读取字符串的剩余部分
    /// </summary>
    public string ReadToEnd()
    {
        var end = _str.Length;
        var value = _str.Substring(_pos, end - _pos);
        _pos = end;
        return value;
    }

    /// <summary>
    /// 读取包含引号的字符串
    /// </summary>
    public string ReadQuoted()
    {
        if (IsEnd()) return "";
        return Peek(1) switch
        {
            "\"" => Skip(1).Read('\"', true)[..^1],
            "\'" => Skip(1).Read('\'', true)[..^1],
            _ => Read(' ')
        };
    }

    private int FixIndex(int index)
    {
        return index < 0 ? _str.Length : index;
    }
}