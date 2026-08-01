using System.Net;
using System.Net.Http;
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
        Assert.Equal("https://rutracker.org/forum/viewtopic.php?t=5956108", hits[0].DetailUrl);
        Assert.Equal("123456", hits[1].Id);
        Assert.Equal("https://rutracker.org/forum/viewtopic.php?t=123456", hits[1].DetailUrl);
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

    [Fact]
    public void LooksLikeCloudflareChallengeBody_DetectsChallengePage()
    {
        const string html = """
            <!DOCTYPE html><html><head><title>Just a moment...</title></head>
            <body><script src="https://challenges.cloudflare.com/turnstile/v0/api.js"></script>
            <div>cdn-cgi/challenge-platform</div></body></html>
            """;

        Assert.True(RuTrackerHtmlParser.LooksLikeCloudflareChallengeBody(html));
        Assert.False(RuTrackerHtmlParser.LooksLikeCloudflareChallengeBody("<html>login_username</html>"));
    }

    [Fact]
    public void LooksLikeCloudflareChallengeBody_IgnoresTrackerPageThatMentionsCloudflare()
    {
        const string html = """
            <html><body>
            <tr data-topic_id="1"><td><a href="viewtopic.php?t=1">Title</a></td></tr>
            <script>/* served via cloudflare */</script>
            </body></html>
            """;

        Assert.False(RuTrackerHtmlParser.LooksLikeCloudflareChallengeBody(html));
        Assert.False(RuTrackerHtmlParser.LooksLikeCloudflareChallengeBody(
            "<html>powered by cloudflare ray</html>"));
    }

    [Fact]
    public void LooksLikeCloudflareChallenge_DetectsCfMitigatedHeader()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
        response.Headers.TryAddWithoutValidation("cf-mitigated", "challenge");

        Assert.True(RuTrackerHtmlParser.LooksLikeCloudflareChallenge(response, body: null));
    }

    [Fact]
    public void LooksLikeCloudflareChallenge_TreatsOpaque403AsChallenge()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
        Assert.True(RuTrackerHtmlParser.LooksLikeCloudflareChallenge(response, "<html>blocked</html>"));
        Assert.False(RuTrackerHtmlParser.LooksLikeCloudflareChallenge(
            response,
            "<html><input name=\"login_username\"/></html>"));
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
