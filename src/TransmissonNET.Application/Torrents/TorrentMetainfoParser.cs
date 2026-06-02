using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Exceptions;
using TransmissonNET.Application.Settings;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Torrents;

public static class TorrentMetainfoParser
{
    private const int MaxTorrentBytes = 10 * 1024 * 1024;

    public static TorrentMetainfoPreviewDto Parse(byte[] data, string? sourceFileName = null)
    {
        if (data.Length == 0)
            throw new SettingsValidationException("Torrent file is empty.");

        if (data.Length > MaxTorrentBytes)
            throw new SettingsValidationException("Torrent file is too large.");

        try
        {
            var reader = new BencodeReader(data);
            var root = reader.ReadDictionary();
            if (!root.TryGetValue("info", out var infoValue) || infoValue is not Dictionary<string, object> info)
                throw new SettingsValidationException("Invalid torrent: missing info.");

            var displayName = GetString(info, "name") ?? sourceFileName ?? "Torrent";
            var files = ExtractFiles(info, displayName);
            var totalSize = files.Sum(file => file.Length);
            var tree = TorrentFileTreeBuilder.Build(files);

            return new TorrentMetainfoPreviewDto(
                displayName,
                sourceFileName ?? displayName,
                totalSize,
                tree);
        }
        catch (SettingsValidationException)
        {
            throw;
        }
        catch (Exception ex) when (ex is InvalidDataException or FormatException)
        {
            throw new SettingsValidationException("Invalid torrent file format.");
        }
    }

    private static IReadOnlyList<TorrentFile> ExtractFiles(Dictionary<string, object> info, string fallbackName)
    {
        if (info.TryGetValue("files", out var filesValue) && AsObjectList(filesValue) is { } fileEntries)
            return ParseMultiFile(fileEntries);

        var length = GetLong(info, "length");
        if (length <= 0)
            throw new SettingsValidationException("Invalid torrent: missing file length.");

        var name = GetString(info, "name") ?? fallbackName;
        return
        [
            new TorrentFile
            {
                Index = 0,
                Name = name,
                Length = length,
                BytesCompleted = 0,
                Wanted = true,
                Priority = 0,
            },
        ];
    }

    private static IReadOnlyList<TorrentFile> ParseMultiFile(IList<object> fileEntries)
    {
        var list = new List<TorrentFile>();
        var index = 0;

        foreach (var entryValue in fileEntries)
        {
            if (entryValue is not Dictionary<string, object> entry)
                continue;

            var length = GetLong(entry, "length");
            if (length <= 0)
                continue;

            var path = GetPath(entry);
            if (string.IsNullOrWhiteSpace(path))
                continue;

            list.Add(new TorrentFile
            {
                Index = index++,
                Name = path,
                Length = length,
                BytesCompleted = 0,
                Wanted = true,
                Priority = 0,
            });
        }

        if (list.Count == 0)
            throw new SettingsValidationException("Invalid torrent: no files in metainfo.");

        return list;
    }

    private static string GetPath(Dictionary<string, object> entry)
    {
        if (!entry.TryGetValue("path", out var pathValue))
            return string.Empty;

        if (pathValue is string single)
            return single;

        if (pathValue is not IList<object> parts)
            return string.Empty;

        var segments = parts.OfType<string>().Where(part => part.Length > 0).ToArray();
        return segments.Length == 0 ? string.Empty : string.Join('/', segments);
    }

    private static IList<object>? AsObjectList(object? value) =>
        value is IList<object> list ? list : null;

    private static string? GetString(Dictionary<string, object> dict, string key) =>
        dict.TryGetValue(key, out var value) ? value switch
        {
            string text => text,
            long number => number.ToString(System.Globalization.CultureInfo.InvariantCulture),
            _ => null,
        } : null;

    private static long GetLong(Dictionary<string, object> dict, string key) =>
        dict.TryGetValue(key, out var value) ? value switch
        {
            long number => number,
            int number => number,
            string text when long.TryParse(text, out var parsed) => parsed,
            _ => 0,
        } : 0;
}
