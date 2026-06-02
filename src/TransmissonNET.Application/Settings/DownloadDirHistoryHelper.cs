namespace TransmissonNET.Application.Settings;

public static class DownloadDirHistoryHelper
{
    public const int MaxCount = 50;

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
}
