using TransmissonNET.Application.Exceptions;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Settings;

internal static class DaemonSessionSettingsValidator
{
    public static void Validate(TransmissionDaemonSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.DownloadDir))
            throw new SettingsValidationException("Download directory is required.");

        if (settings.IncompleteDirEnabled && string.IsNullOrWhiteSpace(settings.IncompleteDir))
            throw new SettingsValidationException("Incomplete directory is required when incomplete folder is enabled.");

        if (settings.PeerLimitGlobal < 0)
            throw new SettingsValidationException("Global peer limit cannot be negative.");

        if (settings.PeerLimitPerTorrent < 0)
            throw new SettingsValidationException("Per-torrent peer limit cannot be negative.");

        if (settings.SpeedLimitDownKbps < 0 || settings.SpeedLimitUpKbps < 0)
            throw new SettingsValidationException("Speed limits cannot be negative.");

        if (settings.SeedRatioLimit < 0)
            throw new SettingsValidationException("Seed ratio limit cannot be negative.");

        if (settings.IdleSeedingLimitMinutes < 0)
            throw new SettingsValidationException("Idle seeding limit cannot be negative.");
    }
}
