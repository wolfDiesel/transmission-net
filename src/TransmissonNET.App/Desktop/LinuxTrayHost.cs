namespace TransmissonNET.App.Desktop;

internal static class LinuxTrayHost
{
    public static ILinuxTrayHost? TryCreate(string? iconPath = null) =>
        AyatanaAppIndicatorTrayHost.TryCreate(iconPath);
}
