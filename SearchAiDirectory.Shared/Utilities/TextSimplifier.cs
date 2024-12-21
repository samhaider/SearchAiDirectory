using System.Text.RegularExpressions;

namespace SearchAiDirectory.Shared.Utilities;

public static partial class RegexHelper
{
    [GeneratedRegex("(?:[^a-z0-9 ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)] private static partial Regex SpecialCharacter();

    public static string TextToSlug(string text)
    {
        return SpecialCharacter().Replace(text, string.Empty).Trim().ToLower().Replace(" ", "-");
    }
}
