using Flandre.Core.Utils;
using Flandre.Framework.Common;
using Flandre.Framework.Routing;

namespace Flandre.Framework.Services;

internal sealed class CommandService
{
    public CommandNode RootNode { get; } = new("");

    public Dictionary<StringShortcut, Command> StringShortcuts { get; } = new();

    public Dictionary<RegexShortcut, Command> RegexShortcuts { get; } = new();

    internal Dictionary<Type, string> TypeFriendlyNames { get; } = new();

    internal Dictionary<Type, TypeResolverDelegate> TypeResolvers { get; } = new();

    internal CommandService()
    {
        AddInternalTypeResolvers();
    }

    public void MapTypeResolver<T>(string? typeFriendlyName, TypeResolverDelegate<T> resolver)
    {
        var type = typeof(T);
        TypeResolvers[type] = (string raw, out object? result) =>
        {
            var suc = resolver(raw, out var res);
            result = res;
            return suc;
        };

        if (typeFriendlyName is not null)
            TypeFriendlyNames[type] = typeFriendlyName;
    }

    public bool TryParseArgumentValue(Type type, string raw, out object? result)
    {
        if (TypeResolvers.TryGetValue(type, out var typeResolver))
            return typeResolver(raw, out result);

        result = null;
        return false;
    }

    public string GetTypeFriendlyName(Type type)
    {
        return TypeFriendlyNames.TryGetValue(type, out var name)
            ? name
            : type.Name;
    }

    internal void Reset()
    {
        RootNode.Clear();
        StringShortcuts.Clear();
        RegexShortcuts.Clear();
        TypeFriendlyNames.Clear();
        TypeResolvers.Clear();
        AddInternalTypeResolvers();
    }

    #region 初始化

    private void AddInternalTypeResolvers()
    {
        MapTypeResolver<int>("整数", int.TryParse);
        MapTypeResolver<long>("整数", long.TryParse);
        MapTypeResolver<byte>("整数", byte.TryParse);
        MapTypeResolver<sbyte>("整数", sbyte.TryParse);
        MapTypeResolver<short>("整数", short.TryParse);

        MapTypeResolver<uint>("正整数", uint.TryParse);
        MapTypeResolver<ulong>("正整数", ulong.TryParse);
        MapTypeResolver<ushort>("正整数", ushort.TryParse);

        MapTypeResolver<double>("小数", double.TryParse);
        MapTypeResolver<float>("小数", float.TryParse);
        MapTypeResolver<decimal>("小数", decimal.TryParse);

        MapTypeResolver<bool>("\"true\"或\"false\"", bool.TryParse);

        MapTypeResolver<char>("字符", char.TryParse);

        // ReSharper disable once RedundantTypeArgumentsOfMethod
        MapTypeResolver<string>("不带空格的文本", (string raw, out string result) =>
        {
            result = new StringParser(raw).ReadQuoted();
            return true;
        });
    }

    #endregion
}
