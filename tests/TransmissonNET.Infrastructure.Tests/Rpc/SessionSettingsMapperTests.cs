using System.Text.Json;
using TransmissonNET.Infrastructure.Rpc;

namespace TransmissonNET.Infrastructure.Tests.Rpc;

public class SessionSettingsMapperTests
{
    [Fact]
    public void Map_ReadsKebabCaseFields()
    {
        using var doc = JsonDocument.Parse(
            """
            {
              "download-dir": "/data/downloads",
              "incomplete-dir": "/data/incomplete",
              "incomplete-dir-enabled": true,
              "trash-original-torrent-files": false,
              "peer-limit-global": 200,
              "peer-limit-per-torrent": 50,
              "speed-limit-down": 100,
              "speed-limit-up": 50,
              "speed-limit-down-enabled": true,
              "speed-limit-up-enabled": false,
              "seedRatioLimit": 2.5,
              "seedRatioLimited": true,
              "idle-seeding-limit": 30,
              "idle-seeding-limit-enabled": true
            }
            """);

        var settings = SessionSettingsMapper.Map(doc.RootElement);

        Assert.Equal("/data/downloads", settings.DownloadDir);
        Assert.Equal("/data/incomplete", settings.IncompleteDir);
        Assert.True(settings.IncompleteDirEnabled);
        Assert.Equal(200, settings.PeerLimitGlobal);
        Assert.Equal(100, settings.SpeedLimitDownKbps);
        Assert.Equal(2.5, settings.SeedRatioLimit);
        Assert.True(settings.SeedRatioLimited);
        Assert.Equal(30, settings.IdleSeedingLimitMinutes);
    }

    [Fact]
    public void ToRpcArguments_UsesTransmissionKeys()
    {
        var settings = new TransmissonNET.Domain.TransmissionDaemonSettings(
            "/dl",
            "/inc",
            true,
            false,
            100,
            25,
            500,
            200,
            true,
            true,
            1.0,
            false,
            15,
            true);

        var args = SessionSettingsMapper.ToRpcArguments(settings);

        Assert.Equal("/dl", args["download-dir"]);
        Assert.Equal(500, args["speed-limit-down"]);
        Assert.Equal(1.0, args["seedRatioLimit"]);
        Assert.True((bool)args["idle-seeding-limit-enabled"]!);
    }
}
