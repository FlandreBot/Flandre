using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Flandre.Core.Utils;
using Flandre.Framework.Attributes;

namespace Flandre.Framework.Common;

internal abstract class CommandShortcut
{
    public string Target { get; }

    public CommandShortcut(string target)
    {
        Target = target;
    }

    public abstract bool TryFormat(string text, [NotNullWhen(true)] out string? resultText);
}

internal class StringShortcut : CommandShortcut
{
    private readonly string _shortcut;

    private readonly bool _allowArguments;

    public StringShortcut(string shortcut, string target, bool allowArguments) : base(target)
    {
        _shortcut = shortcut;
        _allowArguments = allowArguments;
    }

    public StringShortcut(StringShortcutAttribute attr)
        : this(attr.StringShortcut, attr.Target, attr.AllowArguments)
    {
    }

    public override bool TryFormat(string text, [NotNullWhen(true)] out string? resultText)
    {
        if (_allowArguments)
        {
            if (text.StartsWith(_shortcut))
            {
                resultText = $"{Target} {text.TrimStart(_shortcut)}";
                return true;
            }
        }
        else if (text == _shortcut)
        {
            resultText = Target;
            return true;
        }

        resultText = null;
        return false;
    }
}

internal class RegexShortcut : CommandShortcut
{
    private readonly Regex _regex;

    public RegexShortcut(Regex regex, string target) : base(target)
    {
        _regex = regex;
    }

    public RegexShortcut(RegexShortcutAttribute attr)
        : this(attr.RegexShortcut, attr.Target)
    {
    }

    public override bool TryFormat(string text, [NotNullWhen(true)] out string? resultText)
    {
        var match = _regex.Match(text);
        if (match.Success)
        {
            // for (var i = 0; i < match.Groups.Count; i++)
            // {
            //     var group = match.Groups[i];
            //     text = text.Replace($"${i + 1}", group.Value);
            // }
            resultText = _regex.Replace(text, Target);
            return true;
        }

        resultText = null;
        return false;
    }
}
