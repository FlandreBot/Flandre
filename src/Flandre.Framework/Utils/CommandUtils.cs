using Flandre.Core.Utils;

namespace Flandre.Framework.Utils;

internal static class CommandUtils
{
    internal static bool TryParseValue(string section, Type type, out object result)
    {
        result = null!;

        if (type == typeof(int) && int.TryParse(section, out var intVal))
        {
            result = intVal;
            return true;
        }

        if (type == typeof(long) && long.TryParse(section, out var longVal))
        {
            result = longVal;
            return true;
        }

        if (type == typeof(double) && double.TryParse(section, out var doubleVal))
        {
            result = doubleVal;
            return true;
        }

        if (type == typeof(float) && float.TryParse(section, out var floatVal))
        {
            result = floatVal;
            return true;
        }

        if (type == typeof(bool) && bool.TryParse(section, out var boolVal))
        {
            result = boolVal;
            return true;
        }

        if (type == typeof(byte) && byte.TryParse(section, out var byteVal))
        {
            result = byteVal;
            return true;
        }

        if (type == typeof(char) && char.TryParse(section, out var charVal))
        {
            result = charVal;
            return true;
        }

        if (type == typeof(sbyte) && sbyte.TryParse(section, out var sbyteVal))
        {
            result = sbyteVal;
            return true;
        }

        if (type == typeof(short) && short.TryParse(section, out var shortVal))
        {
            result = shortVal;
            return true;
        }

        if (type == typeof(uint) && uint.TryParse(section, out var uintVal))
        {
            result = uintVal;
            return true;
        }

        if (type == typeof(ulong) && ulong.TryParse(section, out var ulongVal))
        {
            result = ulongVal;
            return true;
        }

        if (type == typeof(string))
        {
            // 如果可以最好在外部就 parse 掉字符串，不然会产生多余对象
            result = new StringParser(section).ReadQuoted();
            return true;
        }

        return false;
    }

    internal static string GetTypeDescription(Type type)
    {
        if (type == typeof(int) || type == typeof(long) || type == typeof(byte)
            || type == typeof(sbyte) || type == typeof(short))
            return "整数";

        if (type == typeof(double) || type == typeof(float)) return "小数";

        if (type == typeof(uint) || type == typeof(ulong)) return "正整数";

        if (type == typeof(bool)) return "\"true\"或\"false\"";

        if (type == typeof(char)) return "字符";

        if (type == typeof(string)) return "文本";

        return type.Name;
    }
}