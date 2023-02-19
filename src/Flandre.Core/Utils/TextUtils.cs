namespace Flandre.Core.Utils;

internal static class TextUtils
{
    internal static string RemoveString(this string text, string remove)
    {
        return text.Replace(remove, "");
    }

    internal static string TrimStart(this string source, string value,
        StringComparison comparison = StringComparison.Ordinal)
    {
        if (value == "")
            return source;
        var valueLength = value.Length;
        var startIndex = 0;
        while (source.IndexOf(value, startIndex, comparison) == startIndex)
            startIndex += valueLength;

        return source[startIndex..];
    }

    internal static string TrimEnd(this string source, string value,
        StringComparison comparison = StringComparison.Ordinal)
    {
        if (value == "")
            return source;
        var sourceLength = source.Length;
        var valueLength = value.Length;
        var count = sourceLength;
        while (source.LastIndexOf(value, count, comparison) == count - valueLength)
            count -= valueLength;

        return source[..count];
    }
}