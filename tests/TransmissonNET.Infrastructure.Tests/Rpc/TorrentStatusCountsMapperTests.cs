using System.Text.Json;
using TransmissonNET.Domain;
using TransmissonNET.Infrastructure.Rpc;

namespace TransmissonNET.Infrastructure.Tests.Rpc;

public class TorrentStatusCountsMapperTests
{
    [Fact]
    public void Map_CountsDownloadingAndCompleted()
    {
        const string json = """
            {
              "torrents": [
                { "status": 4, "percentDone": 0.5 },
                { "status": 3, "percentDone": 0 },
                { "status": 6, "percentDone": 1 }
              ]
            }
            """;

        using var doc = JsonDocument.Parse(json);
        var counts = TorrentStatusCountsMapper.Map(doc.RootElement);

        Assert.Equal(2, counts.Downloading);
        Assert.Equal(1, counts.Completed);
    }
}
