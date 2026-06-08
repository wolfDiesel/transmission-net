using TransmissonNET.Application.Settings;

namespace TransmissonNET.Application.Tests.Settings;

public class DownloadDirHistoryHelperTests
{
    [Fact]
    public void Remember_PutsNewPathFirst_AndDeduplicates()
    {
        var history = new[] { "/old", "/other" };

        var next = DownloadDirHistoryHelper.Remember(history, "/new");

        Assert.Equal("/new", next[0]);
        Assert.Equal(3, next.Count);
        Assert.DoesNotContain("/new", next.Skip(1));
    }

    [Fact]
    public void Remember_MovesExistingPathToFront()
    {
        var history = new[] { "/first", "/second" };

        var next = DownloadDirHistoryHelper.Remember(history, "/second");

        Assert.Equal("/second", next[0]);
        Assert.Equal(2, next.Count);
    }

    [Fact]
    public void Remember_CapsAtMaxCount()
    {
        var history = Enumerable.Range(0, DownloadDirHistoryHelper.MaxCount)
            .Select(index => $"/dir{index}")
            .ToArray();

        var next = DownloadDirHistoryHelper.Remember(history, "/new");

        Assert.Equal(DownloadDirHistoryHelper.MaxCount, next.Count);
        Assert.Equal("/new", next[0]);
    }

    [Fact]
    public void MatchesQuery_FiltersByPathOrFolderName()
    {
        Assert.True(DownloadDirHistoryHelper.MatchesQuery("/home/user/Downloads", "user"));
        Assert.True(DownloadDirHistoryHelper.MatchesQuery("/home/user/Downloads", "down"));
        Assert.False(DownloadDirHistoryHelper.MatchesQuery("/home/user/Downloads", "media"));
        Assert.True(DownloadDirHistoryHelper.MatchesQuery("/home/user/Downloads", ""));
    }

    [Fact]
    public void FolderDisplayName_ReturnsLastSegment()
    {
        Assert.Equal("Downloads", DownloadDirHistoryHelper.FolderDisplayName("/home/user/Downloads"));
        Assert.Equal("Downloads", DownloadDirHistoryHelper.FolderDisplayName("/home/user/Downloads/"));
    }
}
