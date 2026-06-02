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
}
