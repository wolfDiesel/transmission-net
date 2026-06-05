using System.Text;
using System.Text.RegularExpressions;

namespace TransmissonNET.Application.Torrents;

public static class TorrentNameWildcardFilter
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);

    public static bool IsMatch(string text, string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return true;

        if (string.IsNullOrEmpty(text))
            return false;

        return ToRegex(NormalizePattern(pattern.Trim())).IsMatch(text);
    }

    private static string NormalizePattern(string pattern)
    {
        if (pattern.Contains('*') || pattern.Contains('?'))
            return pattern;

        return $"*{pattern}*";
    }

    private static Regex ToRegex(string pattern)
    {
        var builder = new StringBuilder("^");
        foreach (var character in pattern)
        {
            switch (character)
            {
                case '*':
                    builder.Append(".*");
                    break;
                case '?':
                    builder.Append('.');
                    break;
                default:
                    builder.Append(Regex.Escape(character.ToString()));
                    break;
            }
        }

        builder.Append('$');
        return new Regex(
            builder.ToString(),
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
            RegexTimeout);
    }
}
