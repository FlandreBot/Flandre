namespace Flandre.Core.Utils;

/// <summary>
/// 字符串解析器
/// </summary>
public class StringParser
{
    private readonly string _str;
    private int _pos;
    private readonly char[] _leftQuotes;
    private readonly char[] _rightQuotes;

    /// <summary>
    /// 当前字符
    /// </summary>
    public char Current => _str[_pos];

    // ReSharper disable SuggestBaseTypeForParameterInConstructor
    /// <summary>
    /// 使用字符串构造解析器
    /// </summary>
    public StringParser(string str, HashSet<char> quoteChars, HashSet<(char Left, char Right)> quotePairs)
    {
        _str = str;
        _leftQuotes = quotePairs.Select(t => t.Left).Concat(quoteChars).Where(c => !char.IsWhiteSpace(c)).ToArray();
        _rightQuotes = quotePairs.Select(t => t.Right).Concat(quoteChars).Where(c => !char.IsWhiteSpace(c)).ToArray();
    }
    // ReSharper restore SuggestBaseTypeForParameterInConstructor

    /// <summary>
    /// 使用字符串构造解析器
    /// </summary>
    public StringParser(string str, params char[] quoteChars) : this(str, quoteChars.ToHashSet(), new HashSet<(char Left, char Right)>()) { }

    /// <summary>
    /// 字符串是否解析完
    /// </summary>
    public bool IsEnd => _pos >= _str.Length;

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
    /// <param name="includeTerminator"></param>
    public StringParser Skip(char terminator, bool includeTerminator = false)
    {
        _pos = IndexOfOrEnd(terminator);
        if (includeTerminator)
            ++_pos;
        return this;
    }

    /// <summary>
    /// 跳到指定的字符位置
    /// </summary>
    /// <param name="predicate">true时停止</param>
    /// <param name="includeTerminator"></param>
    public StringParser SkipWhen(Func<char, bool> predicate, bool includeTerminator = false)
    {
        while (!(IsEnd || predicate(Current)))
            ++_pos;
        if (includeTerminator)
            ++_pos;
        return this;
    }

    /// <summary>
    /// 跳过空白字符
    /// </summary>
    public StringParser SkipWhiteSpaces() => SkipWhen(char.IsWhiteSpace);

    /// <summary>
    /// 读取字符串，但不移动解析器指针
    /// </summary>
    /// <param name="length">读取字符串的长度</param>
    public string Peek(int length) => _str.Substring(_pos, length);

    /// <summary>
    /// 读取字符串，但不移动解析器指针
    /// </summary>
    /// <param name="terminator">终点字符</param>
    /// <param name="includeTerminator"></param>
    public string Peek(char terminator, bool includeTerminator = false)
    {
        var end = IndexOfOrEnd(terminator);
        if (includeTerminator)
            ++end;
        return _str[_pos..end];
    }

    /// <summary>
    /// 读取字符串，但不移动解析器指针
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="includeTerminator"></param>
    public string PeekWhen(Func<char, bool> predicate, bool includeTerminator = false)
    {
        var end = _pos;
        while (!(IsEnd || predicate(Current)))
            ++end;
        if (includeTerminator)
            ++end;
        return _str[_pos..end];
    }

    /// <summary>
    /// 读取字符串，且移动解析器指针
    /// </summary>
    /// <param name="terminator">终点字符，将解析器指针指向该字符</param>
    /// <param name="includeTerminator">同时读取终点字符，解析器指针指向下一字符</param>
    public string Read(char terminator, bool includeTerminator = false)
    {
        var start = _pos;
        _ = Skip(terminator, includeTerminator);
        return _str[start.._pos];
    }

    /// <summary>
    /// 读取字符串，且移动解析器指针
    /// </summary>
    /// <param name="predicate">true时停止读取</param>
    /// <param name="includeTerminator">同时读取终点字符，解析器指针指向下一字符</param>
    public string ReadWhen(Func<char, bool> predicate, bool includeTerminator = false)
    {
        var start = _pos;
        _ = SkipWhen(predicate, includeTerminator);
        return _str[start.._pos];
    }

    /// <summary>
    /// 读取字符串，且移动解析器指针，直至空白字符
    /// </summary>
    public string ReadToWhiteSpace() => ReadWhen(char.IsWhiteSpace);

    /// <summary>
    /// 读取字符串的剩余部分
    /// </summary>
    public string ReadToEnd()
    {
        var value = _str[_pos..];
        _pos = _str.Length;
        return value;
    }

    /// <summary>
    /// 读取包含引号的字符串
    /// </summary>
    public string ReadQuoted()
    {
        if (IsEnd)
            return "";

        var index = Array.IndexOf(_leftQuotes, Current);

        return index is not -1 ? Read(_rightQuotes[index], true)[1..^1] : ReadToWhiteSpace();
    }

    private int IndexOfOrEnd(char value)
    {
        var r = _str.IndexOf(value, _pos);
        return r is -1 ? _str.Length : r;
    }
}
