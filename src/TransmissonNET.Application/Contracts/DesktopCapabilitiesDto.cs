namespace TransmissonNET.Application.Contracts;

public sealed record DesktopCapabilitiesDto(
    bool TraySupported,
    bool TraySettingsAvailable);
