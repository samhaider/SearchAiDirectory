using Humanizer;
using System.Net;
using System.Text.RegularExpressions;

namespace SearchAiDirectory.Shared.Utilities;

public static partial class RegexHelper
{
    [GeneratedRegex("(?:[^a-z0-9 ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)] private static partial Regex SpecialCharacter();
    [GeneratedRegex("<(script|style)[^>]*>.*?</\\1>", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-US")] private static partial Regex ScriptStyleRemover();
    [GeneratedRegex("<[^>]*(>|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)] private static partial Regex HtmlToText();
    [GeneratedRegex(@"\s+")] private static partial Regex WhitespaceNormalizer();
    [GeneratedRegex(@"^(https?|ftp)://[^\s/$.?#].[^\s]*$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)] private static partial Regex UrlValidator();
    
    public static string TextToSlug(string text)
    {
        return SpecialCharacter().Replace(text, string.Empty).Trim().ToLower().Replace(" ", "-");
    }

    public static string HtmlToCleanText(string inputHtml)
    {
        // Remove script and style tags and their content
        string noScriptOrStyle = ScriptStyleRemover().Replace(inputHtml, string.Empty);

        // Remove all HTML tags
        string noHtml = HtmlToText().Replace(noScriptOrStyle, string.Empty);

        // Decode HTML entities
        string decoded = WebUtility.HtmlDecode(noHtml);

        // Normalize whitespace
        string normalized = WhitespaceNormalizer().Replace(decoded, " ").Trim();

        // Humanize the text
        return normalized.Humanize(LetterCasing.Sentence);
    }

    public static bool IsValidUrl(string inputString) => UrlValidator().IsMatch(inputString);
}
