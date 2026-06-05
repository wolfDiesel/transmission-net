using System.Text;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Handlers;

namespace TransmissonNET.Application.Tests.Handlers;

public class InspectTorrentMetainfoFromPathHandlerTests
{
    private const string SampleBencode = "d4:infod6:lengthi12e4:name8:test.txtee";

    [Fact]
    public async Task HandleAsync_ExistingTorrentFile_ReturnsPreviewAndBase64()
    {
        var path = CreateTempTorrent(SampleBencode);
        try
        {
            var handler = new InspectTorrentMetainfoFromPathHandler();
            var result = await handler.HandleAsync(new TorrentMetainfoInspectPathRequestDto(path));

            Assert.Equal(Convert.ToBase64String(Encoding.UTF8.GetBytes(SampleBencode)), result.MetainfoBase64);
            Assert.Equal("test.txt", result.Preview.Name);
            Assert.Equal(12, result.Preview.TotalSize);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task HandleAsync_MissingFile_ThrowsFileNotFound()
    {
        var handler = new InspectTorrentMetainfoFromPathHandler();
        var missing = Path.Combine(Path.GetTempPath(), $"tn-missing-{Guid.NewGuid():N}.torrent");

        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            handler.HandleAsync(new TorrentMetainfoInspectPathRequestDto(missing)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_EmptyPath_ThrowsArgumentException(string path)
    {
        var handler = new InspectTorrentMetainfoFromPathHandler();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(new TorrentMetainfoInspectPathRequestDto(path)));
    }

    [Fact]
    public async Task HandleAsync_NonTorrentExtension_ThrowsArgumentException()
    {
        var path = Path.Combine(Path.GetTempPath(), $"tn-{Guid.NewGuid():N}.txt");
        File.WriteAllText(path, "not a torrent");
        try
        {
            var handler = new InspectTorrentMetainfoFromPathHandler();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                handler.HandleAsync(new TorrentMetainfoInspectPathRequestDto(path)));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string CreateTempTorrent(string bencode)
    {
        var path = Path.Combine(Path.GetTempPath(), $"tn-{Guid.NewGuid():N}.torrent");
        File.WriteAllText(path, bencode);
        return path;
    }
}
