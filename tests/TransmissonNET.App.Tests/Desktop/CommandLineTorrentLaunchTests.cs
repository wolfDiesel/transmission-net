using TransmissonNET.App.Desktop;
using Xunit;

namespace TransmissonNET.App.Tests.Desktop;

public class CommandLineTorrentLaunchTests
{
    [Fact]
    public void FindTorrentPath_IgnoresFlagsAndMissingFiles()
    {
        Assert.Null(CommandLineTorrentLaunch.FindTorrentPath(["--help"]));
        Assert.Null(CommandLineTorrentLaunch.FindTorrentPath(["missing.torrent"]));
    }

    [Fact]
    public void FindTorrentPath_ReturnsExistingTorrentFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"tn-{Guid.NewGuid():N}.torrent");
        File.WriteAllBytes(path, [0x64, 0x38, 0x3a]);

        try
        {
            var found = CommandLineTorrentLaunch.FindTorrentPath([path]);
            Assert.Equal(Path.GetFullPath(path), found);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
