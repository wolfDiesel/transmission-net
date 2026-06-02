using TransmissonNET.Application.Exceptions;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Settings;

internal static class SettingsValidator
{
    public static void Validate(AppSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Daemon.Host))
            throw new SettingsValidationException("Daemon host is required.");

        if (settings.Daemon.Port is < 1 or > 65535)
            throw new SettingsValidationException("Daemon port must be between 1 and 65535.");

        if (string.IsNullOrWhiteSpace(settings.Daemon.RpcPath))
            throw new SettingsValidationException("Daemon RPC path is required.");

        if (settings.Ui.RefreshIntervalSeconds < 1)
            throw new SettingsValidationException("Refresh interval must be at least 1 second.");

        if (settings.Ui.WindowWidth < 320 || settings.Ui.WindowHeight < 240)
            throw new SettingsValidationException("Window size is too small.");

        if (!UiColorSchemes.All.Contains(settings.Ui.ColorScheme))
            throw new SettingsValidationException("Unknown color scheme.");

        if (!UiAppearances.All.Contains(settings.Ui.Appearance))
            throw new SettingsValidationException("Unknown appearance mode.");
    }

    public static void ValidateConnection(DaemonConnection connection)
    {
        if (string.IsNullOrWhiteSpace(connection.Host))
            throw new SettingsValidationException("Daemon host is required.");

        if (connection.Port is < 1 or > 65535)
            throw new SettingsValidationException("Daemon port must be between 1 and 65535.");
    }
}
