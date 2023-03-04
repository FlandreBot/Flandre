using Flandre.Core.Utils;
using Flandre.Framework.Common;

namespace Flandre.Framework.Services;

internal sealed class CommandService
{
    public CommandNode RootCommandNode { get; } = new("");

    public Dictionary<StringShortcut, Command> StringShortcuts { get; } = new();

    public Dictionary<RegexShortcut, Command> RegexShortcuts { get; } = new();

    public Dictionary<Type, TypeParserDelegate> TypeParsers { get; } = new();

    public Dictionary<Type, string> TypeFriendlyNames { get; } = new();

    public CommandService()
    {
        AddInternalTypeParsers();
    }

    public void AddTypeParser<T>(string? typeFriendlyName, TypeParserDelegate<T> parser)
    {
        var type = typeof(T);
        TypeParsers[type] = (string raw, out object? result) =>
        {
            var suc = parser(raw, out var res);
            result = res;
            return suc;
        };

        if (typeFriendlyName is not null)
            TypeFriendlyNames[type] = typeFriendlyName;
    }

    public bool TryParseArgumentValue(Type type, string raw, out object? result)
    {
        if (TypeParsers.TryGetValue(type, out var typeParser))
            return typeParser(raw, out result);

        result = null;
        return false;
    }

    public string GetTypeFriendlyName(Type type)
    {
        return TypeFriendlyNames.TryGetValue(type, out var name)
            ? name
            : type.Name;
    }

    #region 初始化

    private void AddInternalTypeParsers()
    {
        AddTypeParser<int>("整数", int.TryParse);
        AddTypeParser<long>("整数", long.TryParse);
        AddTypeParser<byte>("整数", byte.TryParse);
        AddTypeParser<sbyte>("整数", sbyte.TryParse);
        AddTypeParser<short>("整数", short.TryParse);

        AddTypeParser<uint>("正整数", uint.TryParse);
        AddTypeParser<ulong>("正整数", ulong.TryParse);
        AddTypeParser<ushort>("正整数", ushort.TryParse);

        AddTypeParser<double>("小数", double.TryParse);
        AddTypeParser<float>("小数", float.TryParse);
        AddTypeParser<decimal>("小数", decimal.TryParse);

        AddTypeParser<bool>("\"true\"或\"false\"", bool.TryParse);

        AddTypeParser<char>("字符", char.TryParse);

        // ReSharper disable once RedundantTypeArgumentsOfMethod
        AddTypeParser<string>("不带空格的文本", (string raw, out string result) =>
        {
            result = new StringParser(raw).ReadQuoted();
            return true;
        });
    }

    #endregion
}
