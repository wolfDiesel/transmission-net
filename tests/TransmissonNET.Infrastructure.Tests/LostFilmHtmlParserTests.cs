using TransmissonNET.Providers.LostFilm;
using Xunit;

namespace TransmissonNET.Infrastructure.Tests;

public sealed class LostFilmHtmlParserTests
{
    [Fact]
    public void ParseSearchSeries_SkipsMovies()
    {
        var html = LoadFixture("search-results.html");
        var series = LostFilmHtmlParser.ParseSearchSeries(html);

        Assert.Equal(2, series.Count);
        Assert.Equal("Lost", series[0].Slug);
        Assert.Contains("Lost", series[0].Title);
        Assert.Equal("The_Lost_Room", series[1].Slug);
    }

    [Fact]
    public void ParseSeasonEpisodes_ParsesCodesAndSkipsSeasonPack()
    {
        var html = LoadFixture("seasons.html");
        var hits = LostFilmHtmlParser.ParseSeasonEpisodes(
            html,
            "Lost",
            "Lost",
            "https://www.lostfilm.download/",
            maxEpisodes: 10);

        Assert.Equal(2, hits.Count);
        Assert.Equal("30-6-17", hits[0].Id);
        Assert.Contains("The End", hits[0].Title);
        Assert.Contains("episode_17", hits[0].DetailUrl);
        Assert.Equal("30-6-16", hits[1].Id);
    }

    [Fact]
    public void ParseDownloadRedirectUrl_ReadsMetaRefresh()
    {
        var html = LoadFixture("v-search-redirect.html");
        var url = LostFilmHtmlParser.ParseDownloadRedirectUrl(html, "https://www.lostfilm.download/");
        Assert.Equal("https://www.lostfilm.download/dld?c=30&s=6&e=17", url);
    }

    [Fact]
    public void SelectQualityUrl_PrefersConfiguredQuality()
    {
        var html = LoadFixture("download-page.html");
        var links = LostFilmHtmlParser.ParseQualityLinks(html);
        Assert.Equal(3, links.Count);

        var preferred = LostFilmHtmlParser.SelectQualityUrl(links, "1080");
        Assert.Equal("https://n.tracktor.bio/td/1080.torrent", preferred);

        var fallback = LostFilmHtmlParser.SelectQualityUrl(links, "ZZZ");
        Assert.Equal("https://n.tracktor.bio/td/1080.torrent", fallback);
    }

    private static string LoadFixture(string name)
    {
        var probe = new DirectoryInfo(AppContext.BaseDirectory);
        while (probe is not null)
        {
            var path = Path.Combine(
                probe.FullName,
                "src",
                "TransmissonNET.Providers.LostFilm",
                "Fixtures",
                name);
            if (File.Exists(path))
                return File.ReadAllText(path);
            probe = probe.Parent;
        }

        throw new FileNotFoundException($"Fixture not found: {name}");
    }
}
