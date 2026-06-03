using TransmissonNET.Infrastructure.Desktop;

namespace TransmissonNET.Infrastructure.Tests.Desktop;

public class MimeAppsListWriterTests
{
    [Fact]
    public void SetDefaultHandler_WritesDefaultApplicationsSection()
    {
        var configDir = Path.Combine(Path.GetTempPath(), $"tn-mime-{Guid.NewGuid():N}");
        var home = Path.Combine(configDir, "home");
        Directory.CreateDirectory(Path.Combine(home, ".config"));
        var previousHome = Environment.GetEnvironmentVariable("HOME");
        Environment.SetEnvironmentVariable("HOME", home);

        try
        {
            MimeAppsListWriter.SetDefaultHandler("application/x-bittorrent", "transmission-net.desktop");

            var content = File.ReadAllText(Path.Combine(home, ".config", "mimeapps.list"));
            Assert.Contains("[Default Applications]", content);
            Assert.Contains("application/x-bittorrent=transmission-net.desktop;", content);
        }
        finally
        {
            Environment.SetEnvironmentVariable("HOME", previousHome);
            Directory.Delete(configDir, recursive: true);
        }
    }
}

public class DesktopFileEncodingTests
{
    [Fact]
    public void Instance_DoesNotEmitUtf8Bom()
    {
        var path = Path.Combine(Path.GetTempPath(), $"tn-desktop-{Guid.NewGuid():N}.desktop");
        File.WriteAllText(path, "[Desktop Entry]\nType=Application\n", DesktopFileEncoding.Instance);

        try
        {
            var bytes = File.ReadAllBytes(path);
            Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);
            Assert.Equal((byte)'[', bytes[0]);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
