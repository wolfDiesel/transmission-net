using TransmissonNET.Domain;
using TransmissonNET.Infrastructure.Settings;

namespace TransmissonNET.Infrastructure.Tests.Settings;

public class JsonSettingsStoreTests
{
    [Fact]
    public async Task SaveAndLoad_Roundtrip()
    {
        var path = Path.Combine(Path.GetTempPath(), $"transmissionnet-test-{Guid.NewGuid():N}.json");
        var store = new JsonSettingsStore(path);

        var settings = new AppSettings(
            new DaemonConnection("10.0.0.5", 9092, "/transmission/rpc", "admin", "pass"),
            new UiSettings(5, 1024, 768, TorrentTableSettings.CreateDefault(), UiColorSchemes.Teal, UiAppearances.Light));

        await store.SaveAsync(settings);
        var loaded = await store.LoadAsync();

        Assert.Equal(settings.Daemon.Host, loaded.Daemon.Host);
        Assert.Equal(settings.Daemon.Port, loaded.Daemon.Port);
        Assert.Equal(settings.Daemon.Password, loaded.Daemon.Password);
        Assert.Equal(settings.Ui.RefreshIntervalSeconds, loaded.Ui.RefreshIntervalSeconds);
        Assert.Equal(settings.Ui.ColorScheme, loaded.Ui.ColorScheme);
        Assert.Equal(settings.Ui.Appearance, loaded.Ui.Appearance);
        Assert.Equal(settings.Ui.TorrentTable.SortColumnId, loaded.Ui.TorrentTable.SortColumnId);
        Assert.Equal(
            settings.Ui.TorrentTable.Columns.Count,
            loaded.Ui.TorrentTable.Columns.Count);

        File.Delete(path);
    }

    [Fact]
    public async Task SaveAndLoad_PersistsDownloadDirHistory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"transmissionnet-history-{Guid.NewGuid():N}.json");
        var store = new JsonSettingsStore(path);

        var settings = new AppSettings(
            new DaemonConnection("127.0.0.1", 9091, "/transmission/rpc", string.Empty, string.Empty),
            new UiSettings(
                3,
                1280,
                800,
                TorrentTableSettings.CreateDefault(),
                DownloadDirHistory: new[] { "/downloads", "/media" }));

        await store.SaveAsync(settings);
        var loaded = await store.LoadAsync();

        Assert.Equal(new[] { "/downloads", "/media" }, loaded.Ui.DownloadDirHistory);

        File.Delete(path);
    }

    [Fact]
    public async Task Load_WhenMissing_ReturnsDefaults()
    {
        var path = Path.Combine(Path.GetTempPath(), $"transmissionnet-missing-{Guid.NewGuid():N}.json");
        var store = new JsonSettingsStore(path);

        var settings = await store.LoadAsync();

        Assert.Equal("127.0.0.1", settings.Daemon.Host);
        Assert.Equal(9091, settings.Daemon.Port);
        Assert.Equal(3, settings.Ui.RefreshIntervalSeconds);
        Assert.Equal(TorrentTableColumnIds.Name, settings.Ui.TorrentTable.SortColumnId);
    }

    [Fact]
    public async Task SaveAndLoad_PersistsLanguage()
    {
        var path = Path.Combine(Path.GetTempPath(), $"transmissionnet-lang-{Guid.NewGuid():N}.json");
        var store = new JsonSettingsStore(path);

        var settings = new AppSettings(
            new DaemonConnection("127.0.0.1", 9091, "/transmission/rpc", string.Empty, string.Empty),
            new UiSettings(3, 1280, 800, TorrentTableSettings.CreateDefault(), Language: UiLanguages.Russian));

        await store.SaveAsync(settings);
        var loaded = await store.LoadAsync();

        Assert.Equal(UiLanguages.Russian, loaded.Ui.Language);

        File.Delete(path);
    }
}
