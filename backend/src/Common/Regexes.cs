using System.Text.RegularExpressions;

namespace Personal.Budget.App.Common;

public static partial class Regexes
{
    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    public static partial Regex HexColorRegex { get; }
}
