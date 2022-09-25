using Flandre.Core.Utils;

namespace Flandre.Core.Tests.UtilsTests;

public class CommandUtilsTests
{
    [Theory]
    [InlineData("<alpha:double>", true, "double", default(double))]
    [InlineData("<beta=1234>", true, "string", "")] // string arg type defaults to empty, not null
    [InlineData("[gamma]", false, "string", "")]
    [InlineData("[]", false, "string", "")]
    [InlineData(" [epsilon: bool = true]  ", false, "bool", true)]
    [InlineData("[f: bool = 12345678]  ", false, "bool", false)] // wrong default value type
    [InlineData("[g = 'a string']", false, "string", "a string")] // test string with spaces
    public void TestParseParameterSection(string section, bool isRequired, string expectedType, object defaultValue)
    {
        var info = CommandUtils.ParseParameterSection(section);
        Assert.Equal(isRequired, info.IsRequired);
        Assert.Equal(expectedType, info.Type);
        Assert.Equal(defaultValue, info.DefaultValue);
    }

    [Theory]
    [InlineData("bool", false)]
    [InlineData("byte", (byte)0)]
    [InlineData("char", '\0')]
    [InlineData("double", 0d)]
    [InlineData("float", 0f)]
    [InlineData("int", 0)]
    [InlineData("long", 0L)]
    [InlineData("sbyte", (sbyte)0)]
    [InlineData("short", (short)0)]
    [InlineData("uint", 0u)]
    [InlineData("ulong", 0ul)]
    [InlineData("ushort", (ushort)0)]
    [InlineData("string", "")]
    [InlineData("asd", false, "bool")] // fallback
    public void TestGetTypeDefaultValue(string type, object expectDefault, string fallbackType = "string")
    {
        Assert.Equal(expectDefault, CommandUtils.GetTypeDefaultValue(type, fallbackType));
    }

    [Theory]
    [InlineData("114514", "string", "114514", true)]
    [InlineData("\"114514\"", "string", "114514", true)] // use quotes
    [InlineData("\"114514\"", "string", "\"114514\"", true, false)] // use quotes, but not parse
    [InlineData("false", "bool", false, true)]
    [InlineData("114", "byte", (byte)114, true)]
    [InlineData("s", "char", 's', true)]
    [InlineData("114.514", "double", 114.514d, true)]
    [InlineData("1145.14", "float", 1145.14f, true)]
    [InlineData("114514", "int", 114514, true)]
    [InlineData("114514", "long", 114514L, true)]
    [InlineData("114", "sbyte", (sbyte)114, true)]
    [InlineData("1145", "short", (short)1145, true)]
    [InlineData("11451400", "uint", 11451400u, true)]
    [InlineData("11451400", "ulong", 11451400ul, true)]
    [InlineData("114", "ushort", (ushort)114, true)]
    [InlineData("114514", "bool", 114514, false)] // can't parse
    public void TestTryParseType(string section, string type, object expectedValue, bool canParse,
        bool parseString = true)
    {
        var result = CommandUtils.TryParseType(section, type, out var value, parseString);
        Assert.Equal(canParse, result);
        if (result)
            Assert.Equal(expectedValue, value);
    }
}