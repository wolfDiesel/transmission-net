using TransmissonNET.Infrastructure.Desktop;

namespace TransmissonNET.Infrastructure.Tests.Desktop;

public class LinuxTorrentFileAssociationServiceTests
{
    [Fact]
    public void BuildDesktopEntry_IncludesExecPathAndTorrentMime()
    {
        var entry = LinuxTorrentFileAssociationService.BuildDesktopEntry("/opt/TransmissionNET.App");

        Assert.Contains("Exec=\"/opt/TransmissionNET.App\" %f", entry);
        Assert.Contains("MimeType=application/x-bittorrent;", entry);
        Assert.Contains("Name=TransmissionNET", entry);
        Assert.Contains("StartupWMClass=TransmissionNET", entry);
    }

    [Fact]
    public void BuildDesktopEntry_EscapesQuotesInExecPath()
    {
        var entry = LinuxTorrentFileAssociationService.BuildDesktopEntry("/opt/with\"quote/App");

        Assert.Contains("Exec=\"/opt/with\\\"quote/App\" %f", entry);
    }

    [Fact]
    public void FindMatchingDesktopEntries_IgnoresCanonicalFileWithoutMatchingContent()
    {
        var dir = CreateTempApplicationsDir();
        var exec = "/opt/TransmissionNET.App";
        var canonical = Path.Combine(dir, LinuxTorrentFileAssociationService.DesktopFileName);
        File.WriteAllText(
            canonical,
            """
            [Desktop Entry]
            Type=Application
            Name=Other App
            Exec=/usr/bin/other %f
            """);

        try
        {
            Assert.Empty(LinuxTorrentFileAssociationService.FindMatchingDesktopEntries(dir, exec));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void FindMatchingDesktopEntries_ReturnsAllMatchingEntriesSortedByConfidence()
    {
        var dir = CreateTempApplicationsDir();
        var exec = "/opt/TransmissionNET.App";
        var weak = Path.Combine(dir, "weak.desktop");
        var strong = Path.Combine(dir, "strong.desktop");
        File.WriteAllText(
            weak,
            """
            [Desktop Entry]
            Type=Application
            Name=TransmissionNET
            Exec=/other/TransmissonNET.App %f
            """);
        File.WriteAllText(
            strong,
            """
            [Desktop Entry]
            Type=Application
            Name=TransmissionNET
            Exec=/opt/TransmissionNET.App %f
            StartupWMClass=TransmissionNET
            """);

        try
        {
            var matches = LinuxTorrentFileAssociationService.FindMatchingDesktopEntries(dir, exec);
            Assert.Equal(2, matches.Count);
            Assert.Equal(strong, matches[0].FilePath);
            Assert.Equal(weak, matches[1].FilePath);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void FindExistingDesktopEntryPath_ReturnsMatchingShortcutByParsedContent()
    {
        var dir = CreateTempApplicationsDir();
        var exec = "/opt/TransmissionNET.App";
        var custom = Path.Combine(dir, "custom-transmission-net.desktop");
        File.WriteAllText(
            custom,
            """
            [Desktop Entry]
            Type=Application
            Name=TransmissionNET
            Exec=/old/path/TransmissonNET.App %f
            StartupWMClass=TransmissionNET
            """);

        try
        {
            Assert.Equal(custom, LinuxTorrentFileAssociationService.FindExistingDesktopEntryPath(dir, exec));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void ResolveDesktopEntryPathForWrite_UsesExistingShortcutPath()
    {
        var dir = CreateTempApplicationsDir();
        var exec = "/opt/TransmissionNET.App";
        var custom = Path.Combine(dir, "legacy.desktop");
        File.WriteAllText(
            custom,
            """
            [Desktop Entry]
            Name=TransmissionNET
            Exec=/old/TransmissonNET.App %f
            """);

        try
        {
            Assert.Equal(custom, LinuxTorrentFileAssociationService.ResolveDesktopEntryPathForWrite(dir, exec));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void ResolveDesktopEntryPathForWrite_CreatesCanonicalPathWhenMissing()
    {
        var dir = CreateTempApplicationsDir();
        var exec = "/opt/TransmissionNET.App";

        try
        {
            var path = LinuxTorrentFileAssociationService.ResolveDesktopEntryPathForWrite(dir, exec);
            Assert.Equal(Path.Combine(dir, LinuxTorrentFileAssociationService.DesktopFileName), path);
            Assert.False(File.Exists(path));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void ResolvePrimaryDesktopEntryPath_PrefersCanonicalName()
    {
        var paths = new[]
        {
            "/home/user/.local/share/applications/legacy.desktop",
            "/home/user/.local/share/applications/transmission-net.desktop",
        };

        Assert.Equal(paths[1], LinuxTorrentFileAssociationService.ResolvePrimaryDesktopEntryPath(paths));
    }

    private static string CreateTempApplicationsDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"tn-apps-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        return dir;
    }
}

public class DesktopEntryParserTests
{
    [Fact]
    public void TryParse_ReadsDesktopEntryGroupFields()
    {
        var parsed = DesktopEntryParser.TryParse(
            "/tmp/test.desktop",
            """
            [Desktop Entry]
            Type=Application
            Name=TransmissionNET
            Exec=/opt/TransmissionNET.App %f
            StartupWMClass=TransmissionNET
            Hidden=false
            """);

        Assert.NotNull(parsed);
        Assert.Equal("Application", parsed.Type);
        Assert.Equal("TransmissionNET", parsed.Name);
        Assert.Equal("/opt/TransmissionNET.App %f", parsed.Exec);
        Assert.Equal("TransmissionNET", parsed.StartupWmClass);
        Assert.False(parsed.Hidden);
    }

    [Fact]
    public void TryParse_IgnoresOtherGroups()
    {
        var parsed = DesktopEntryParser.TryParse(
            "/tmp/test.desktop",
            """
            [Desktop Action]
            Name=Open
            Exec=/bin/false
            [Desktop Entry]
            Name=TransmissionNET
            Exec=/opt/TransmissionNET.App %f
            """);

        Assert.NotNull(parsed);
        Assert.Equal("TransmissionNET", parsed.Name);
        Assert.Equal("/opt/TransmissionNET.App %f", parsed.Exec);
    }

    [Fact]
    public void ExtractExecutablePath_UnquotesExecField()
    {
        Assert.Equal(
            "/opt/with space/App",
            DesktopEntryParser.ExtractExecutablePath("\"/opt/with space/App\" %f"));
    }
}
