using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Exceptions;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Handlers;

public sealed class ExecuteTorrentActionHandler(
    ISettingsStore settingsStore,
    ITransmissionClient transmissionClient)
{
    public async Task HandleAsync(TorrentActionDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Ids is null || dto.Ids.Count == 0)
            throw new SettingsValidationException("At least one torrent id is required.");

        var settings = await settingsStore.LoadAsync(cancellationToken);
        var ids = dto.Ids.ToArray();

        switch (dto.Action.Trim().ToLowerInvariant())
        {
            case "start":
                await transmissionClient.StartTorrentsAsync(settings.Daemon, ids, cancellationToken);
                break;
            case "stop":
                await transmissionClient.StopTorrentsAsync(settings.Daemon, ids, cancellationToken);
                break;
            case "remove":
                await transmissionClient.RemoveTorrentsAsync(
                    settings.Daemon,
                    ids,
                    dto.DeleteLocalData,
                    cancellationToken);
                break;
            case "verify":
                await transmissionClient.VerifyTorrentsAsync(settings.Daemon, ids, cancellationToken);
                break;
            case "set-priority":
                await transmissionClient.SetTorrentBandwidthPriorityAsync(
                    settings.Daemon,
                    ids,
                    ParsePriority(dto.Priority),
                    cancellationToken);
                break;
            case "move":
                if (string.IsNullOrWhiteSpace(dto.Location))
                    throw new SettingsValidationException("Location is required.");
                await transmissionClient.SetTorrentLocationAsync(
                    settings.Daemon,
                    ids,
                    dto.Location.Trim(),
                    dto.Move,
                    cancellationToken);
                break;
            case "rename-path":
                if (ids.Length != 1)
                    throw new SettingsValidationException("Rename requires exactly one torrent.");
                if (string.IsNullOrWhiteSpace(dto.Path))
                    throw new SettingsValidationException("Path is required.");
                if (string.IsNullOrWhiteSpace(dto.Name))
                    throw new SettingsValidationException("Name is required.");
                var newName = dto.Name.Trim();
                if (newName.Contains('/') || newName.Contains('\\'))
                    throw new SettingsValidationException("Name must not contain path separators.");
                await transmissionClient.RenameTorrentPathAsync(
                    settings.Daemon,
                    ids[0],
                    dto.Path.Trim(),
                    newName,
                    cancellationToken);
                break;
            default:
                throw new SettingsValidationException($"Unknown torrent action: {dto.Action}");
        }
    }

    private static TorrentBandwidthPriority ParsePriority(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "low" => TorrentBandwidthPriority.Low,
            "high" => TorrentBandwidthPriority.High,
            "normal" or "" or null => TorrentBandwidthPriority.Normal,
            _ => throw new SettingsValidationException("Priority must be low, normal, or high."),
        };
}
