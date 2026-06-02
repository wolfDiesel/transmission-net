using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Exceptions;

namespace TransmissonNET.Application.Settings;

public static class TorrentRenameBatchValidator
{
    public static IReadOnlyList<TorrentRenameOperationDto> ValidateAndNormalize(
        TorrentRenameBatchRequestDto request)
    {
        if (request.Operations is null || request.Operations.Count == 0)
            throw new SettingsValidationException("At least one rename operation is required.");

        var normalized = new List<TorrentRenameOperationDto>(request.Operations.Count);
        var targetKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var op in request.Operations)
        {
            var path = op.Path?.Trim() ?? string.Empty;
            var name = op.Name?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(path))
                throw new SettingsValidationException("Path is required for each operation.");

            if (string.IsNullOrEmpty(name))
                throw new SettingsValidationException("Name is required for each operation.");

            if (name.Contains('/') || name.Contains('\\'))
                throw new SettingsValidationException("Name must not contain path separators.");

            var parent = GetParentPath(path);
            var targetKey = $"{parent}\0{name}";

            if (!targetKeys.Add(targetKey))
                throw new SettingsValidationException($"Duplicate target name in folder: {name}");

            normalized.Add(new TorrentRenameOperationDto(path, name));
        }

        return normalized;
    }

    private static string GetParentPath(string path)
    {
        var lastSlash = path.LastIndexOf('/');
        return lastSlash < 0 ? string.Empty : path[..lastSlash];
    }
}
