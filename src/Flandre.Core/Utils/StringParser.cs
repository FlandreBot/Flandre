namespace Flandre.Core.Utils;

internal class StringParser
{
    private readonly string _str;
    private int _pos;

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

    internal char Current()
    {
        return _str[_pos];
    }

    internal string Peek(int length)
    {
        return _str.Substring(_pos, length);
    }

    internal char PeekChar()
    {
        return _str[_pos + 1];
    }

    internal string Peek(char terminator)
    {
        var end = FixIndex(_str.IndexOf(terminator, _pos));
        return _str.Substring(_pos, end - _pos);
    }

    internal string Read(char terminator, int offset = 0, bool includeTerminator = false)
    {
        _pos += offset;
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

    private int FixIndex(int index)
    {
        return index < 0 ? _str.Length : index;
    }
}