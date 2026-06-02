using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Settings;

namespace TransmissonNET.Application.Handlers;

public sealed class GetSettingsHandler(ISettingsStore settingsStore)
{
    public async Task<AppSettingsDto> HandleAsync(CancellationToken cancellationToken = default)
    {
        var settings = await settingsStore.LoadAsync(cancellationToken);
        return SettingsMapper.ToDto(settings, maskPassword: true);
    }
}
