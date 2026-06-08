namespace TransmissonNET.Application.Settings;

public static class DownloadDirHistoryHelper
{
    public const int MaxCount = 500;

    public static IReadOnlyList<string> Remember(IReadOnlyList<string>? history, string path)
    {
        var trimmed = path.Trim();
        if (trimmed.Length == 0)
            return history?.ToArray() ?? Array.Empty<string>();

        var list = new List<string> { trimmed };
        foreach (var item in history ?? Array.Empty<string>())
        {
            if (string.Equals(item, trimmed, StringComparison.Ordinal))
                continue;

            list.Add(item);
            if (list.Count >= MaxCount)
                break;
        }

        return list;
    }

    public static string FolderDisplayName(string path)
    {
        var normalized = path.Replace('\\', '/').TrimEnd('/');
        var parts = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[^1] : path;
    }

    public static bool MatchesQuery(string path, string query)
    {
        var trimmedQuery = query.Trim();
        if (trimmedQuery.Length == 0)
            return true;

        if (path.Contains(trimmedQuery, StringComparison.OrdinalIgnoreCase))
            return true;

        return FolderDisplayName(path).Contains(trimmedQuery, StringComparison.OrdinalIgnoreCase);
    }
}
