using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Settings;

namespace TransmissonNET.Application.Handlers;

public sealed class SaveDaemonSessionSettingsHandler(
    ISettingsStore settingsStore,
    ITransmissionClient transmissionClient)
{
    public async Task<DaemonSessionSettingsDto> HandleAsync(
        DaemonSessionSettingsDto dto,
        CancellationToken cancellationToken = default)
    {
        var domain = DaemonSessionSettingsMapper.ToDomain(dto);
        DaemonSessionSettingsValidator.Validate(domain);

        var settings = await settingsStore.LoadAsync(cancellationToken);
        await transmissionClient.SetDaemonSessionSettingsAsync(
            settings.Daemon,
            domain,
            cancellationToken);
        return DaemonSessionSettingsMapper.ToDto(domain);
    }
}
