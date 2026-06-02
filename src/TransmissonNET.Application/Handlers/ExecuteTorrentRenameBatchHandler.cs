using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Exceptions;
using TransmissonNET.Application.Settings;

namespace TransmissonNET.Application.Handlers;

public sealed class ExecuteTorrentRenameBatchHandler(
    ISettingsStore settingsStore,
    ITransmissionClient transmissionClient)
{
    public async Task<TorrentRenameBatchResultDto> HandleAsync(
        int torrentId,
        TorrentRenameBatchRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var operations = TorrentRenameBatchValidator.ValidateAndNormalize(request);
        var settings = await settingsStore.LoadAsync(cancellationToken);
        var failures = new List<TorrentRenameFailureDto>();
        var applied = 0;

        var ordered = operations
            .OrderByDescending(op => op.Path.Count(c => c == '/'))
            .ThenBy(op => op.Path, StringComparer.Ordinal)
            .ToList();

        foreach (var op in ordered)
        {
            try
            {
                await transmissionClient.RenameTorrentPathAsync(
                    settings.Daemon,
                    torrentId,
                    op.Path,
                    op.Name,
                    cancellationToken);
                applied++;
            }
            catch (DaemonConnectionException ex)
            {
                failures.Add(new TorrentRenameFailureDto(op.Path, ex.Message));
            }
        }

        return new TorrentRenameBatchResultDto(applied, failures);
    }
}
