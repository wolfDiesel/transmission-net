using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Settings;

namespace TransmissonNET.Application.Handlers;

public sealed class SaveSettingsHandler(ISettingsStore settingsStore)
{
    public async Task<AppSettingsDto> HandleAsync(
        AppSettingsDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await settingsStore.LoadAsync(cancellationToken);
        var settings = SettingsMapper.ToDomain(dto, existing);
        SettingsValidator.Validate(settings);
        await settingsStore.SaveAsync(settings, cancellationToken);
        return SettingsMapper.ToDto(settings, maskPassword: true);
    }
}
