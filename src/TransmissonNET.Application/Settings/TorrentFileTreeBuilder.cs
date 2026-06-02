using TransmissonNET.Application.Contracts;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Settings;

public static class TorrentFileTreeBuilder
{
    public static IReadOnlyList<TorrentFileNodeDto> Build(IReadOnlyList<TorrentFile> files)
    {
        if (files.Count == 0)
            return Array.Empty<TorrentFileNodeDto>();

        if (files.Count == 1 && !files[0].Name.Contains('/'))
            return [ToFileNode(files[0])];

        var roots = new Dictionary<string, MutableNode>(StringComparer.Ordinal);

        foreach (var file in files)
        {
            var parts = file.Name.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                continue;

            var currentMap = roots;
            MutableNode? current = null;
            var path = string.Empty;

            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                path = string.IsNullOrEmpty(path) ? part : $"{path}/{part}";
                var isFile = i == parts.Length - 1;

                if (!currentMap.TryGetValue(part, out current))
                {
                    current = new MutableNode(part, path, !isFile);
                    currentMap[part] = current;
                }

                if (isFile)
                    current.AttachFile(file);
                else
                    currentMap = current.Children;
            }
        }

        return roots.Values
            .OrderBy(node => node.Name, StringComparer.OrdinalIgnoreCase)
            .Select(node => node.ToDto())
            .ToList();
    }

    private static TorrentFileNodeDto ToFileNode(TorrentFile file) =>
        new(
            Path.GetFileName(file.Name),
            file.Name,
            false,
            file.Index,
            file.Length,
            file.BytesCompleted,
            file.Wanted,
            file.Priority,
            Array.Empty<TorrentFileNodeDto>());

    private sealed class MutableNode(string name, string path, bool isFolder)
    {
        public string Name { get; } = name;
        public string Path { get; } = path;
        public bool IsFolder { get; } = isFolder;
        public Dictionary<string, MutableNode> Children { get; } = new(StringComparer.Ordinal);
        public TorrentFile? File { get; private set; }
        public long Length { get; private set; }
        public long BytesCompleted { get; private set; }
        public bool? Wanted { get; private set; }
        public int? Priority { get; private set; }

        public void AttachFile(TorrentFile file)
        {
            File = file;
            Length = file.Length;
            BytesCompleted = file.BytesCompleted;
            Wanted = file.Wanted;
            Priority = file.Priority;
        }

        public TorrentFileNodeDto ToDto()
        {
            if (!IsFolder && File is not null)
                return ToFileNode(File);

            var childDtos = Children.Values
                .OrderBy(child => child.Name, StringComparer.OrdinalIgnoreCase)
                .Select(child => child.ToDto())
                .ToList();

            var length = childDtos.Sum(child => child.Length);
            var bytesCompleted = childDtos.Sum(child => child.BytesCompleted);

            return new TorrentFileNodeDto(
                Name,
                Path,
                true,
                null,
                length,
                bytesCompleted,
                null,
                null,
                childDtos);
        }
    }
}
