namespace TransmissonNET.Infrastructure.Desktop;

internal sealed record ParsedDesktopEntry(
    string FilePath,
    string? Type,
    string? Name,
    string? Exec,
    string? StartupWmClass,
    bool Hidden);

internal static class DesktopEntryParser
{
    public static ParsedDesktopEntry? TryParseFile(string filePath)
    {
        try
        {
            var content = File.ReadAllText(filePath);
            return TryParse(filePath, content);
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    public static ParsedDesktopEntry? TryParse(string filePath, string content)
    {
        if (!TryParseDesktopEntryGroup(content, out var fields))
            return null;

        return new ParsedDesktopEntry(
            filePath,
            GetField(fields, "Type"),
            GetPrimaryName(fields),
            GetField(fields, "Exec"),
            GetField(fields, "StartupWMClass"),
            IsHidden(fields));
    }

    private static bool TryParseDesktopEntryGroup(string content, out Dictionary<string, string> fields)
    {
        fields = new Dictionary<string, string>(StringComparer.Ordinal);
        var inDesktopEntry = false;

        foreach (var rawLine in content.Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                inDesktopEntry = line.Equals("[Desktop Entry]", StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (!inDesktopEntry)
                continue;

            var separator = line.IndexOf('=');
            if (separator <= 0)
                continue;

            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim();
            fields[key] = value;
        }

        return fields.Count > 0;
    }

    private static string? GetPrimaryName(Dictionary<string, string> fields)
    {
        if (fields.TryGetValue("Name", out var name) && !string.IsNullOrWhiteSpace(name))
            return name;

        foreach (var pair in fields)
        {
            if (pair.Key.StartsWith("Name[", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(pair.Value))
                return pair.Value;
        }

        return null;
    }

    private static string? GetField(Dictionary<string, string> fields, string key) =>
        fields.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;

    private static bool IsHidden(Dictionary<string, string> fields) =>
        fields.TryGetValue("Hidden", out var hidden)
        && (hidden.Equals("true", StringComparison.OrdinalIgnoreCase)
            || hidden == "1");

    public static string? ExtractExecutablePath(string? execField)
    {
        if (string.IsNullOrWhiteSpace(execField))
            return null;

        var value = execField.Trim();
        if (value.Length == 0)
            return null;

        string executable;
        if (value.StartsWith('"'))
        {
            var endQuote = value.IndexOf('"', 1);
            if (endQuote < 1)
                return null;

            executable = value[1..endQuote];
        }
        else
        {
            var end = value.IndexOf(' ');
            executable = end < 0 ? value : value[..end];
        }

        executable = executable.Trim();
        return executable.Length == 0 ? null : executable;
    }
}
