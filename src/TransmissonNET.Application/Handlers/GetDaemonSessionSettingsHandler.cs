using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Settings;

namespace TransmissonNET.Application.Handlers;

public sealed class GetDaemonSessionSettingsHandler(
    ISettingsStore settingsStore,
    ITransmissionClient transmissionClient)
{
    public async Task<DaemonSessionSettingsDto> HandleAsync(CancellationToken cancellationToken = default)
    {
        var settings = await settingsStore.LoadAsync(cancellationToken);
        var session = await transmissionClient.GetDaemonSessionSettingsAsync(
            settings.Daemon,
            cancellationToken);
        return DaemonSessionSettingsMapper.ToDto(session);
    }
}
