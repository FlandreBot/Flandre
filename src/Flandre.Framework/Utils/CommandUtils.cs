using Flandre.Core.Common;
using Flandre.Core.Utils;
using static System.Collections.Specialized.BitVector32;

namespace Flandre.Framework.Utils;

/// <summary>
/// 
/// </summary>
/// <param name="raw"></param>
/// <returns></returns>
public delegate object? TypeParserDelegate(in string raw);

internal static class CommandUtils
{
    #region .NET 7 IParsable<T>

#if NET7_0_OR_GREATER
    /// <summary>
    /// 
    /// </summary>
    /// <param name="raw"></param>
    /// <returns></returns>
    public delegate object? TypeParserDelegate(in string raw);

    public static Dictionary<Type, TypeParserDelegate> TypeParsers { get; } = new();

    static CommandUtils()
    {
        TypeParsers.AddIParseable<int>()
            .AddIParseable<uint>()
            .AddIParseable<short>()
            .AddIParseable<ushort>()
            .AddIParseable<long>()
            .AddIParseable<ulong>()
            .AddIParseable<sbyte>()
            .AddIParseable<byte>()
            .AddIParseable<float>()
            .AddIParseable<double>()
            .AddIParseable<decimal>()
            .AddParser<bool>(BoolParser)
            .AddParser<string>(StringParser);

        object? BoolParser(in string raw) => bool.TryParse(raw, out var tmp) ? tmp : null;
        object? StringParser(in string raw) => throw new NotImplementedException();
    }

    public static Dictionary<Type, TypeParserDelegate> AddIParseable<T>(this Dictionary<Type, TypeParserDelegate> dict)
        where T : IParsable<T> =>
        dict.AddParser(typeof(T), Parse<T>);

    public static Dictionary<Type, TypeParserDelegate> AddEnum<T>(this Dictionary<Type, TypeParserDelegate> dict)
        where T : struct, Enum =>
        dict.AddParser(typeof(T), Enum<T>);

    public static Dictionary<Type, TypeParserDelegate> AddParser<T>(this Dictionary<Type, TypeParserDelegate> dict, TypeParserDelegate parser) =>
        dict.AddParser(typeof(T), parser);

    public static Dictionary<Type, TypeParserDelegate> AddParser(this Dictionary<Type, TypeParserDelegate> dict, Type type, TypeParserDelegate parser)
    {
        dict.Add(type, parser);
        return dict;
    }

    public static object? Enum<TEnum>(in string raw) where TEnum : struct, Enum
        => System.Enum.TryParse(raw, true, out TEnum tmp) && System.Enum.IsDefined(tmp) ? tmp : null;

    public static object? Parse<T>(in string raw) where T : IParsable<T>
        => T.TryParse(raw, null, out var tmp) ? tmp : null;
#endif

    #endregion

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

        if (type == typeof(double) || type == typeof(float))
            return "小数";

        if (type == typeof(uint) || type == typeof(ulong))
            return "正整数";

        if (type == typeof(bool))
            return "\"true\"或\"false\"";

        if (type == typeof(char))
            return "字符";

        if (type == typeof(string))
            return "文本";

        return type.Name;
    }
}
