using TransmissonNET.Providers.Kinozal;
using Xunit;

namespace TransmissonNET.Infrastructure.Tests;

public sealed class KinozalHtmlParserTests
{
    [Fact]
    public void ParseSearchResults_ExtractsHitsFromFixture()
    {
        var html = LoadFixture("browse-results.html");
        var hits = KinozalHtmlParser.ParseSearchResults(html, "https://kinozal.me/");

        Assert.Equal(2, hits.Count);
        Assert.Equal("1922424", hits[0].Id);
        Assert.Contains("Ubuntu", hits[0].Title);
        Assert.Equal(3_650_722_202L, hits[0].SizeBytes);
        Assert.Contains("1922424", hits[0].DetailUrl);
        Assert.Equal("1560368", hits[1].Id);
        Assert.Equal(1_503_238_554L, hits[1].SizeBytes);
    }

    [Theory]
    [InlineData("3.4 ГБ", 3_650_722_202L)]
    [InlineData("1.4 ГБ", 1_503_238_554L)]
    [InlineData("350.5 МБ", 367_525_888L)]
    public void ParseSizeBytes_ParsesRussianUnits(string text, long expected)
    {
        Assert.Equal(expected, KinozalHtmlParser.ParseSizeBytes(text));
    }

    private static string LoadFixture(string name)
    {
        var probe = new DirectoryInfo(AppContext.BaseDirectory);
        while (probe is not null)
        {
            var path = Path.Combine(
                probe.FullName,
                "src",
                "TransmissonNET.Providers.Kinozal",
                "Fixtures",
                name);
            if (File.Exists(path))
                return File.ReadAllText(path);
            probe = probe.Parent;
        }

        throw new FileNotFoundException($"Fixture not found: {name}");
    }
}
