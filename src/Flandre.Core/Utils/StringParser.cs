namespace Flandre.Core.Utils;

internal class StringParser
{
    private readonly string _str;
    private int _pos;

    internal char Current => _str[_pos];

    internal StringParser(string str)
    {
        _str = str;
    }

    internal bool IsEnd()
    {
        return _pos >= _str.Length;
    }

    internal StringParser Skip(int length)
    {
        _pos += length;
        return this;
    }

    internal StringParser SkipSpaces()
    {
        while (!IsEnd())
        {
            if (_str[_pos] != ' ') break;
            _pos++;
        }

        return this;
    }

    internal string Peek(int length)
    {
        return _str.Substring(_pos, length);
    }

    internal string Peek(char terminator)
    {
        var end = FixIndex(_str.IndexOf(terminator, _pos));
        return _str.Substring(_pos, end - _pos);
    }

    internal string Read(char terminator, bool includeTerminator = false)
    {
        var end = FixIndex(_str.IndexOf(terminator, _pos));
        var value = _str.Substring(_pos, end - _pos);
        _pos += end - _pos;

        if (includeTerminator)
        {
            value += terminator;
            _pos++;
        }

        return value;
    }

    internal string ReadToEnd()
    {
        var end = _str.Length;
        var value = _str.Substring(_pos, end - _pos);
        _pos += end - _pos;
        return value;
    }

    internal string ReadQuoted()
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