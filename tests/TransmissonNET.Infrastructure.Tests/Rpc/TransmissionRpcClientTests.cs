using System.Net;
using System.Text;
using RichardSzalay.MockHttp;
using TransmissonNET.Domain;
using TransmissonNET.Infrastructure.Rpc;

namespace TransmissonNET.Infrastructure.Tests.Rpc;

public class TransmissionRpcClientTests
{
    private static readonly DaemonConnection Connection = new(
        "127.0.0.1", 9091, "/transmission/rpc", "user", "pass");

    [Fact]
    public async Task CallAsync_When409_RetriesWithSessionId()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .Respond(_ =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.Conflict);
                response.Headers.Add(SessionHeader, "session-abc");
                return response;
            });

        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .WithHeaders(SessionHeader, "session-abc")
            .Respond("application/json", SessionSuccessJson(rpcVersion: 17));

        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .WithHeaders(SessionHeader, "session-abc")
            .Respond("application/json", SessionSpeedsJson());

        var client = new TransmissionRpcClient(mockHttp.ToHttpClient());
        var session = await client.GetSessionAsync(Connection);

        Assert.Equal(17, session.RpcVersion);
        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task CallAsync_SendsBasicAuth_WhenCredentialsSet()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .WithHeaders("Authorization", "Basic dXNlcjpwYXNz")
            .Respond("application/json", SessionSuccessJson(rpcVersion: 16));

        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .WithHeaders("Authorization", "Basic dXNlcjpwYXNz")
            .Respond("application/json", SessionSpeedsJson());

        var client = new TransmissionRpcClient(mockHttp.ToHttpClient());
        await client.GetSessionAsync(Connection);

        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetTorrentsAsync_MapsFields()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .Respond("application/json", SessionSuccessJson(rpcVersion: 17));

        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .Respond("application/json", SessionSpeedsJson());

        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .Respond("application/json", TorrentsSuccessJson());

        var client = new TransmissionRpcClient(mockHttp.ToHttpClient());
        var torrents = await client.GetTorrentsAsync(
            new DaemonConnection("127.0.0.1", 9091, "/transmission/rpc", "", ""));

        Assert.Single(torrents);
        var t = torrents[0];
        Assert.Equal(1, t.Id);
        Assert.Equal("Test Torrent", t.Name);
        Assert.Equal(TorrentStatus.Downloading, t.Status);
        Assert.Equal(0.5, t.PercentDone);
        Assert.Equal(1700000000, t.AddedDate);
        Assert.Equal("/downloads", t.DownloadDir);
        Assert.Equal(TorrentBandwidthPriority.High, t.BandwidthPriority);
    }

    [Fact]
    public async Task GetTorrentDetailsAsync_MapsFiles()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .Respond("application/json", SessionSuccessJson(rpcVersion: 17));

        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .Respond("application/json", SessionSpeedsJson());

        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .Respond("application/json", TorrentDetailsSuccessJson());

        var client = new TransmissionRpcClient(mockHttp.ToHttpClient());
        var details = await client.GetTorrentDetailsAsync(Connection, 1);

        Assert.NotNull(details);
        Assert.Equal(1, details!.Id);
        Assert.Equal("Demo", details.Name);
        Assert.Equal(2, details.Files.Count);
        Assert.Equal("folder/a.mkv", details.Files[0].Name);
        Assert.True(details.Files[0].Wanted);
        Assert.Equal(500, details.Files[1].BytesCompleted);
        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task RenameTorrentPathAsync_SendsTorrentRenamePath()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .Respond(_ =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(SessionSuccessJson(rpcVersion: 17), Encoding.UTF8, "application/json"),
                };
                response.Headers.Add(SessionHeader, "session-abc");
                return response;
            });

        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .Respond("application/json", SessionSpeedsJson());

        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .With(req =>
            {
                var body = req.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                return body.Contains("torrent-rename-path")
                    && body.Contains("\"path\":\"folder/a.mkv\"")
                    && body.Contains("\"name\":\"b.mkv\"");
            })
            .Respond("application/json", """{"result":"success","arguments":{"id":1,"path":"folder/b.mkv","name":"b.mkv"}}""");

        var client = new TransmissionRpcClient(mockHttp.ToHttpClient());
        await client.RenameTorrentPathAsync(Connection, 1, "folder/a.mkv", "b.mkv");

        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task SetTorrentFilePriorityAsync_SendsPriorityHigh()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .Respond(_ =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(SessionSuccessJson(rpcVersion: 17), Encoding.UTF8, "application/json"),
                };
                response.Headers.Add(SessionHeader, "session-abc");
                return response;
            });

        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .Respond("application/json", SessionSpeedsJson());

        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .With(req =>
            {
                var body = req.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                return body.Contains("torrent-set")
                    && body.Contains("\"priority-high\":[2]");
            })
            .Respond("application/json", """{"result":"success","arguments":{}}""");

        var client = new TransmissionRpcClient(mockHttp.ToHttpClient());
        await client.SetTorrentFilePriorityAsync(Connection, 1, [2], TorrentBandwidthPriority.High);

        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task StartTorrentsAsync_SendsTorrentStart()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .Respond(_ =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(SessionSuccessJson(rpcVersion: 17), Encoding.UTF8, "application/json"),
                };
                response.Headers.Add(SessionHeader, "session-abc");
                return response;
            });

        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .Respond("application/json", SessionSpeedsJson());

        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .WithHeaders(SessionHeader, "session-abc")
            .Respond("application/json", """{"result":"success","arguments":{}}""");

        var client = new TransmissionRpcClient(mockHttp.ToHttpClient());
        await client.StartTorrentsAsync(Connection, [7]);

        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetDaemonSessionSettingsAsync_MapsFields()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .Respond("application/json", SessionSuccessJson(rpcVersion: 17));

        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .Respond("application/json", SessionSpeedsJson());

        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .Respond("application/json", DaemonSessionSettingsJson());

        var client = new TransmissionRpcClient(mockHttp.ToHttpClient());
        var settings = await client.GetDaemonSessionSettingsAsync(Connection);

        Assert.Equal("/var/downloads", settings.DownloadDir);
        Assert.True(settings.IncompleteDirEnabled);
        Assert.Equal(100, settings.SpeedLimitDownKbps);
        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task SetDaemonSessionSettingsAsync_CallsSessionSet()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .Respond("application/json", SessionSuccessJson(rpcVersion: 17));

        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .Respond("application/json", SessionSpeedsJson());

        mockHttp.Expect(HttpMethod.Post, Connection.RpcUrl)
            .With(req =>
            {
                var body = req.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                return body.Contains("session-set") && body.Contains("download-dir");
            })
            .Respond("application/json", """{"result":"success","arguments":{}}""");

        var client = new TransmissionRpcClient(mockHttp.ToHttpClient());
        await client.SetDaemonSessionSettingsAsync(
            Connection,
            new TransmissionDaemonSettings(
                "/new",
                "/inc",
                true,
                false,
                200,
                50,
                0,
                0,
                false,
                false,
                0,
                false,
                0,
                false));

        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public void RpcMethodNaming_UsesKebab_WhenRpcVersion17()
    {
        var naming = new RpcMethodNaming();
        naming.SetRpcVersion(17);

        Assert.Equal("session-get", naming.SessionGet);
        Assert.Equal("torrent-get", naming.TorrentGet);
        Assert.Equal("session-set", naming.SessionSet);
    }

    private const string SessionHeader = "X-Transmission-Session-Id";

    private static string SessionSuccessJson(int rpcVersion) =>
        $$"""
          {
            "result": "success",
            "arguments": {
              "version": "4.0.0",
              "rpc-version": {{rpcVersion}}
            }
          }
          """;

    private static string SessionSpeedsJson() =>
        """
          {
            "result": "success",
            "arguments": {
              "download-speed": 1000,
              "upload-speed": 500
            }
          }
          """;

    private static string DaemonSessionSettingsJson() =>
        """
          {
            "result": "success",
            "arguments": {
              "download-dir": "/var/downloads",
              "incomplete-dir": "/var/incomplete",
              "incomplete-dir-enabled": true,
              "trash-original-torrent-files": false,
              "peer-limit-global": 200,
              "peer-limit-per-torrent": 50,
              "speed-limit-down": 100,
              "speed-limit-up": 50,
              "speed-limit-down-enabled": true,
              "speed-limit-up-enabled": false,
              "seedRatioLimit": 2,
              "seedRatioLimited": true,
              "idle-seeding-limit": 30,
              "idle-seeding-limit-enabled": true
            }
          }
          """;

    private static string TorrentDetailsSuccessJson() =>
        """
          {
            "result": "success",
            "arguments": {
              "torrents": [
                {
                  "id": 1,
                  "name": "Demo",
                  "status": 4,
                  "percentDone": 0.5,
                  "rateDownload": 1000,
                  "rateUpload": 500,
                  "eta": 120,
                  "totalSize": 1000000,
                  "addedDate": 1700000000,
                  "doneDate": 0,
                  "startDate": 1700000100,
                  "uploadRatio": 0.5,
                  "peersConnected": 3,
                  "leftUntilDone": 500000,
                  "downloadedEver": 500000,
                  "uploadedEver": 250000,
                  "queuePosition": 1,
                  "downloadDir": "/downloads",
                  "bandwidthPriority": 0,
                  "error": 0,
                  "errorString": "",
                  "comment": "note",
                  "creator": "Transmission",
                  "dateCreated": 1699999999,
                  "hashString": "abc",
                  "pieceSize": 262144,
                  "isPrivate": false,
                  "files": [
                    { "name": "folder/a.mkv", "length": 1000, "bytesCompleted": 1000 },
                    { "name": "folder/b.mkv", "length": 2000, "bytesCompleted": 500 }
                  ],
                  "fileStats": [
                    { "wanted": true, "priority": 0, "bytesCompleted": 1000 },
                    { "wanted": false, "priority": -1, "bytesCompleted": 500 }
                  ]
                }
              ]
            }
          }
          """;

    private static string TorrentsSuccessJson() =>
        """
          {
            "result": "success",
            "arguments": {
              "torrents": [
                {
                  "id": 1,
                  "name": "Test Torrent",
                  "status": 4,
                  "percentDone": 0.5,
                  "rateDownload": 1000,
                  "rateUpload": 500,
                  "eta": 120,
                  "totalSize": 1000000,
                  "addedDate": 1700000000,
                  "doneDate": 0,
                  "startDate": 1700000100,
                  "uploadRatio": 0.5,
                  "peersConnected": 3,
                  "leftUntilDone": 500000,
                  "downloadedEver": 500000,
                  "uploadedEver": 250000,
                  "queuePosition": 1,
                  "downloadDir": "/downloads",
                  "bandwidthPriority": 1
                }
              ]
            }
          }
          """;
}
