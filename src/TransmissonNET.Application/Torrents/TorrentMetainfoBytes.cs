using TransmissonNET.Application.Exceptions;

namespace TransmissonNET.Application.Torrents;

public static class TorrentMetainfoBytes
{
    public static byte[] FromBase64(string metainfoBase64)
    {
        if (string.IsNullOrWhiteSpace(metainfoBase64))
            throw new SettingsValidationException("Torrent data is required.");

        try
        {
            return Convert.FromBase64String(metainfoBase64.Trim());
        }
        catch (FormatException)
        {
            throw new SettingsValidationException("Invalid torrent data encoding.");
        }
    }
}
