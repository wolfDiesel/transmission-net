namespace TransmissonNET.Infrastructure.Desktop;

internal static class TransmissionNetDesktopMatcher
{
    public static IReadOnlyList<ParsedDesktopEntry> FindMatches(string applicationsDir, string currentExecPath)
    {
        if (!Directory.Exists(applicationsDir))
            return [];

        var currentFullPath = Path.GetFullPath(currentExecPath);
        var ranked = new List<(ParsedDesktopEntry Entry, int Score)>();

        foreach (var filePath in Directory.EnumerateFiles(applicationsDir, "*.desktop"))
        {
            var parsed = DesktopEntryParser.TryParseFile(filePath);
            if (parsed is null)
                continue;

            var score = ScoreEntry(parsed, currentFullPath);
            if (score > 0)
                ranked.Add((parsed, score));
        }

        return ranked
            .OrderByDescending(pair => pair.Score)
            .ThenBy(pair => pair.Entry.FilePath, StringComparer.OrdinalIgnoreCase)
            .Select(pair => pair.Entry)
            .ToList();
    }

    public static bool IsMatch(ParsedDesktopEntry entry, string currentExecPath) =>
        ScoreEntry(entry, Path.GetFullPath(currentExecPath)) > 0;

    private static int ScoreEntry(ParsedDesktopEntry entry, string currentFullPath)
    {
        if (entry.Hidden)
            return 0;

        if (entry.Type is not null
            && !entry.Type.Equals("Application", StringComparison.OrdinalIgnoreCase)
            && !entry.Type.Equals("Link", StringComparison.OrdinalIgnoreCase))
            return 0;

        var score = 0;

        if (string.Equals(entry.StartupWmClass, LinuxTorrentFileAssociationService.StartupWmClass, StringComparison.Ordinal))
            score += 100;

        if (string.Equals(entry.Name, LinuxTorrentFileAssociationService.ApplicationName, StringComparison.Ordinal))
            score += 60;

        if (ExecMatchesCurrentApp(entry.Exec, currentFullPath))
            score += 80;

        return score;
    }

    private static bool ExecMatchesCurrentApp(string? execField, string currentFullPath)
    {
        var execPath = DesktopEntryParser.ExtractExecutablePath(execField);
        if (string.IsNullOrWhiteSpace(execPath))
            return false;

        if (execPath.Contains(LinuxTorrentFileAssociationService.AppExecutableBaseName, StringComparison.OrdinalIgnoreCase))
            return true;

        try
        {
            var execFullPath = Path.GetFullPath(execPath);
            if (execFullPath.Equals(currentFullPath, StringComparison.Ordinal))
                return true;
        }
        catch (ArgumentException)
        {
            return false;
        }

        var currentName = Path.GetFileName(currentFullPath);
        var execName = Path.GetFileName(execPath);
        if (!string.IsNullOrEmpty(currentName)
            && execName.Equals(currentName, StringComparison.OrdinalIgnoreCase))
            return true;

        var appImage = Environment.GetEnvironmentVariable("APPIMAGE");
        if (!string.IsNullOrWhiteSpace(appImage))
        {
            try
            {
                if (Path.GetFullPath(execPath).Equals(Path.GetFullPath(appImage), StringComparison.Ordinal))
                    return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        return false;
    }
}
