namespace TransmissonNET.App;

internal static class LinuxDisplayBootstrap
{
    public static void Configure()
    {
        if (!OperatingSystem.IsLinux())
            return;

        SetIfUnset("WEBKIT_DISABLE_DMABUF_RENDERER", "1");

        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPIMAGE")))
        {
            SetIfUnset("GDK_BACKEND", "x11");
            SetIfUnset("WEBKIT_DISABLE_SANDBOX", "1");
            SetIfUnset("WEBKIT_DISABLE_COMPOSITING_MODE", "1");
        }

        if (string.Equals(Environment.GetEnvironmentVariable("GSK_RENDERER"), "vulkan", StringComparison.OrdinalIgnoreCase))
            SetIfUnset("GSK_RENDERER", "ngl");

        Console.WriteLine(
            "Linux: applied WebKit/Wayland workarounds (WEBKIT_DISABLE_DMABUF_RENDERER=1).");
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPIMAGE")))
            Console.WriteLine(
                "If the window still crashes, run: GDK_BACKEND=x11 dotnet run --project src/TransmissonNET.App");
    }

    private static void SetIfUnset(string name, string value)
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name)))
            Environment.SetEnvironmentVariable(name, value);
    }
}
