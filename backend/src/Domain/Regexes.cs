using System.Text.RegularExpressions;

namespace Domain;

public static partial class Regexes
{
    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    public static partial Regex HexColorRegex { get; }
}
