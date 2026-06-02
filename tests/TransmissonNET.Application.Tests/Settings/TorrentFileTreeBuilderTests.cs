using TransmissonNET.Application.Settings;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Tests.Settings;

public class TorrentFileTreeBuilderTests
{
    [Fact]
    public void Build_SingleFile_ReturnsLeaf()
    {
        var tree = TorrentFileTreeBuilder.Build(
        [
            new TorrentFile
            {
                Index = 0,
                Name = "readme.txt",
                Length = 100,
                BytesCompleted = 50,
                Wanted = true,
                Priority = 0,
            },
        ]);

        Assert.Single(tree);
        Assert.False(tree[0].IsFolder);
        Assert.Equal("readme.txt", tree[0].Name);
        Assert.Equal(50, tree[0].BytesCompleted);
    }

    [Fact]
    public void Build_NestedPaths_GroupsIntoFolders()
    {
        var tree = TorrentFileTreeBuilder.Build(
        [
            new TorrentFile { Index = 0, Name = "dir/a.mkv", Length = 1000, BytesCompleted = 1000, Wanted = true },
            new TorrentFile { Index = 1, Name = "dir/b.mkv", Length = 2000, BytesCompleted = 500, Wanted = true },
        ]);

        Assert.Single(tree);
        var dir = tree[0];
        Assert.True(dir.IsFolder);
        Assert.Equal("dir", dir.Name);
        Assert.Equal(3000, dir.Length);
        Assert.Equal(1500, dir.BytesCompleted);
        Assert.Equal(2, dir.Children.Count);
    }
}
