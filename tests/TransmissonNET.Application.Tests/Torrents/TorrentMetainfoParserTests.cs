using System.Text;
using TransmissonNET.Application.Exceptions;
using TransmissonNET.Application.Torrents;

namespace TransmissonNET.Application.Tests.Torrents;

public class TorrentMetainfoParserTests
{
    [Fact]
    public void Parse_SingleFileTorrent()
    {
        var data = Encoding.UTF8.GetBytes("d4:infod6:lengthi12e4:name8:test.txtee");

        var preview = TorrentMetainfoParser.Parse(data, "sample.torrent");

        Assert.Equal("test.txt", preview.Name);
        Assert.Equal("sample.torrent", preview.FileName);
        Assert.Equal(12, preview.TotalSize);
        Assert.Single(preview.FileTree);
        Assert.False(preview.FileTree[0].IsFolder);
        Assert.Equal(12, preview.FileTree[0].Length);
    }

    [Fact]
    public void Parse_MultiFileTorrent()
    {
        const string bencode =
            "d4:infod5:filesld6:lengthi10e4:pathl4:dir15:a.txteed6:lengthi20e4:pathl4:dir15:b.txteee4:name6:bundleee";

        var preview = TorrentMetainfoParser.Parse(Encoding.UTF8.GetBytes(bencode));

        Assert.Equal("bundle", preview.Name);
        Assert.Equal(30, preview.TotalSize);
        Assert.True(preview.FileTree[0].IsFolder);
        Assert.Equal(2, preview.FileTree[0].Children.Count);
    }

    [Fact]
    public void Parse_InvalidData_Throws()
    {
        Assert.Throws<SettingsValidationException>(() => TorrentMetainfoParser.Parse([1, 2, 3]));
    }
}
