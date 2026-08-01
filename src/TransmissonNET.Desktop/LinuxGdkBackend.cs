using System.Runtime.InteropServices;

namespace TransmissonNET.Desktop;

public static class LinuxGdkBackend
{
    public static void ApplyWebKitGtkWorkarounds()
    {
        if (!OperatingSystem.IsLinux())
            return;

        ForceX11ForGtk();
        SetEnv("WEBKIT_DISABLE_DMABUF_RENDERER", "1");
        SetEnv("WEBKIT_DISABLE_COMPOSITING_MODE", "1");
    }

    public static void ForceX11ForGtk()
    {
        if (!OperatingSystem.IsLinux())
            return;

        SetEnv("GDK_BACKEND", "x11");
    }

    private static void SetEnv(string name, string value)
    {
        setenv(name, value, 1);
        Environment.SetEnvironmentVariable(name, value);
    }

    [DllImport("libc", SetLastError = true)]
    private static extern int setenv(string name, string value, int overwrite);
}
