using Flandre.Core.Utils;
using Flandre.Framework.Attributes;
using Microsoft.Extensions.Logging;

namespace Flandre.Framework.Utils;

internal static class CommandUtils
{
    internal static ParameterInfo ParseParameterSection(string section, string defaultType = "string",
        string cmdName = "")
    {
        var info = new ParameterInfo();
        section = section.Trim();

        if (section[0] == '<')
            info.IsRequired = true;

        var innerRight = section[1..^1].Split('=');
        var innerLeft = innerRight[0].Split(':');
        info.Name = innerLeft[0].Trim();
        info.Type = innerLeft.Length > 1 ? innerLeft[1].Trim().ToLower() : defaultType;

        info.DefaultValue = GetTypeDefaultValue(info.Type, defaultType);

        // 默认值
        if (innerRight.Length > 1)
        {
            if (TryParseType(innerRight[1].Trim(), info.Type, out var result))
                info.DefaultValue = result;
            else
                LogUtils.GetTempLogger<FlandreApp>().LogWarning(
                    "The default value's type of argument {ArgumentName} cannot be match, in command {CommandName}.",
                    info.Name, cmdName);
        }

        return info;
    }

    internal static object GetTypeDefaultValue(string type, string fallbackType = "string")
    {
        return type switch
        {
            "bool" => default(bool),
            "byte" => default(byte),
            "char" => default(char),
            "double" => default(double),
            "float" => default(float),
            "int" => default(int),
            "long" => default(long),
            "sbyte" => default(sbyte),
            "short" => default(short),
            "uint" => default(uint),
            "ulong" => default(ulong),
            "ushort" => default(ushort),
            "string" => "",

            // string or other
            _ => GetTypeDefaultValue(fallbackType)
        };
    }

    internal static bool TryParseType(string section, string type, out object result, bool parseString = true)
    {
        result = section;

        switch (type)
        {
            case "string":
                if (!parseString) break;
                var parser = new StringParser(section);
                result = parser.ReadQuoted();
                break;

            case "bool":
                if (bool.TryParse(section, out var boolVal)) result = boolVal;
                else return false;
                break;

            case "byte":
                if (byte.TryParse(section, out var byteVal)) result = byteVal;
                else return false;
                break;

            case "char":
                if (char.TryParse(section, out var charVal)) result = charVal;
                else return false;
                break;

            case "double":
                if (double.TryParse(section, out var doubleVal)) result = doubleVal;
                else return false;
                break;

            case "float":
                if (float.TryParse(section, out var floatVal)) result = floatVal;
                else return false;
                break;

            case "int":
                if (int.TryParse(section, out var intVal)) result = intVal;
                else return false;
                break;

            case "long":
                if (long.TryParse(section, out var longVal)) result = longVal;
                else return false;
                break;

            case "sbyte":
                if (sbyte.TryParse(section, out var sbyteVal)) result = sbyteVal;
                else return false;
                break;

            case "short":
                if (short.TryParse(section, out var shortVal)) result = shortVal;
                else return false;
                break;

            case "uint":
                if (uint.TryParse(section, out var uintVal)) result = uintVal;
                else return false;
                break;

            case "ulong":
                if (ulong.TryParse(section, out var ulongVal)) result = ulongVal;
                else return false;
                break;

            case "ushort":
                if (ushort.TryParse(section, out var ushortVal)) result = ushortVal;
                else return false;
                break;

            default:
                LogUtils.GetTempLogger<FlandreApp>().LogWarning(
                    "Cannot match the argument type {ArgumentType}, please check the command definition.",
                    type);
                break;
        }

        return true;
    }

    internal static string NormalizeCommandDefinition(string source)
    {
        return string.Join('.', source.Split('.',
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
    }
}