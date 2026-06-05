using TransmissonNET.Desktop;
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
        var path = CreateTempTorrent();

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

    [Fact]
    public void FindTorrentPath_IsCaseInsensitiveForExtension()
    {
        var path = Path.Combine(Path.GetTempPath(), $"tn-{Guid.NewGuid():N}.TORRENT");
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

    [Fact]
    public void FindTorrentPath_SkipsFlagsAndReturnsFirstExistingTorrent()
    {
        var first = CreateTempTorrent();
        var second = CreateTempTorrent();

        try
        {
            var found = CommandLineTorrentLaunch.FindTorrentPath(["--verbose", first, second]);
            Assert.Equal(Path.GetFullPath(first), found);
        }
        finally
        {
            File.Delete(first);
            File.Delete(second);
        }
    }

    [Fact]
    public void FindTorrentPath_IgnoresNonTorrentFiles()
    {
        var torrent = CreateTempTorrent();
        var text = Path.Combine(Path.GetTempPath(), $"tn-{Guid.NewGuid():N}.txt");
        File.WriteAllText(text, "notes");

        try
        {
            var found = CommandLineTorrentLaunch.FindTorrentPath([text, torrent]);
            Assert.Equal(Path.GetFullPath(torrent), found);
        }
        finally
        {
            File.Delete(torrent);
            File.Delete(text);
        }
    }

    private static string CreateTempTorrent()
    {
        var path = Path.Combine(Path.GetTempPath(), $"tn-{Guid.NewGuid():N}.torrent");
        File.WriteAllBytes(path, [0x64, 0x38, 0x3a]);
        return path;
    }
}
