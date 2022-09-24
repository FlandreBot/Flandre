namespace Flandre.Core.Utils;

internal static class TextUtils
{
    internal static string RemoveString(this string text, string remove)
    {
        return text.Replace(remove, "");
    }
}