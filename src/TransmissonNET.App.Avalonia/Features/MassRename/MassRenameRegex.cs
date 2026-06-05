using System.Text.RegularExpressions;

namespace TransmissonNET.App.Avalonia.Features.MassRename;

internal static class MassRenameRegex
{
    public static string NormalizeFlags(string flags) =>
        new string(flags.Where(c => "gimsuy".Contains(c)).ToArray());

    public static (bool Ok, Regex? Regex, string Error) Compile(string pattern, string flags)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return (false, null, "Enter a regex pattern");

        try
        {
            var options = RegexOptions.CultureInvariant;
            var normalized = NormalizeFlags(flags);
            if (normalized.Contains('i'))
                options |= RegexOptions.IgnoreCase;
            return (true, new Regex(pattern, options), string.Empty);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public static bool Matches(Regex regex, string text)
    {
        var options = regex.Options;
        var probe = new Regex(regex.ToString(), options);
        return probe.IsMatch(text);
    }
}
