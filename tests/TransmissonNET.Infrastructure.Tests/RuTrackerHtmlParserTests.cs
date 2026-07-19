using TransmissonNET.Providers.RuTracker;
using Xunit;

namespace TransmissonNET.Infrastructure.Tests;

public sealed class RuTrackerHtmlParserTests
{
    [Fact]
    public void ParseSearchResults_ExtractsHitsFromFixture()
    {
        var html = LoadFixture("search-results.html");
        var hits = RuTrackerHtmlParser.ParseSearchResults(html, "https://rutracker.org");

        Assert.Equal(2, hits.Count);
        Assert.Equal("5956108", hits[0].Id);
        Assert.Equal("Ubuntu 24.04 LTS Desktop amd64", hits[0].Title);
        Assert.Equal(5_046_586_573L, hits[0].SizeBytes); // 4.7 GB rounded
        Assert.Contains("5956108", hits[0].DetailUrl);
        Assert.Equal("123456", hits[1].Id);
        Assert.NotNull(hits[1].SizeBytes);
    }

    [Theory]
    [InlineData("4.7 GB", 5_046_586_573L)]
    [InlineData("350.5 MB", 367_525_888L)]
    [InlineData("1024 KB", 1_048_576L)]
    public void ParseSizeBytes_ParsesUnits(string text, long expected)
    {
        Assert.Equal(expected, RuTrackerHtmlParser.ParseSizeBytes(text));
    }

    private static string LoadFixture(string name)
    {
        var probe = new DirectoryInfo(AppContext.BaseDirectory);
        while (probe is not null)
        {
            var path = Path.Combine(
                probe.FullName,
                "src",
                "TransmissonNET.Providers.RuTracker",
                "Fixtures",
                name);
            if (File.Exists(path))
                return File.ReadAllText(path);
            probe = probe.Parent;
        }

        throw new FileNotFoundException($"Fixture not found: {name}");
    }
}
