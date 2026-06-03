using System.Text.Json;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Settings;
using TransmissonNET.Domain;

namespace TransmissonNET.Infrastructure.Settings;

public sealed class JsonSettingsStore : ISettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly string _filePath;

    public JsonSettingsStore()
        : this(GetDefaultPath())
    {
    }

    public JsonSettingsStore(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
            return CreateDefault();

        await using var stream = File.OpenRead(_filePath);
        var file = await JsonSerializer.DeserializeAsync<SettingsFile>(stream, JsonOptions, cancellationToken);

        return file?.ToDomain() ?? CreateDefault();
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var file = SettingsFile.FromDomain(settings);
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, file, JsonOptions, cancellationToken);
    }

    public static string GetDefaultPath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, ".config", "TransmissonNET", "settings.json");
    }

    private static AppSettings CreateDefault() =>
        new(
            new DaemonConnection("127.0.0.1", 9091, "/transmission/rpc", string.Empty, string.Empty),
            new UiSettings(
                3,
                1280,
                800,
                TorrentTableSettings.CreateDefault(),
                UiColorSchemes.Default,
                UiAppearances.Default));

    private sealed class SettingsFile
    {
        public DaemonFile? Daemon { get; set; }
        public UiFile? Ui { get; set; }

        public AppSettings ToDomain()
        {
            var torrentTable = Ui?.TorrentTable?.ToDomain() ?? TorrentTableSettings.CreateDefault();

            return new AppSettings(
                new DaemonConnection(
                    Daemon?.Host ?? "127.0.0.1",
                    Daemon?.Port ?? 9091,
                    Daemon?.RpcPath ?? "/transmission/rpc",
                    Daemon?.Username ?? string.Empty,
                    Daemon?.Password ?? string.Empty),
                new UiSettings(
                    Ui?.RefreshIntervalSeconds ?? 3,
                    Ui?.WindowWidth ?? 1280,
                    Ui?.WindowHeight ?? 800,
                    torrentTable,
                    NormalizeColorScheme(Ui?.ColorScheme),
                    NormalizeAppearance(Ui?.Appearance),
                    NormalizeDownloadDirHistory(Ui?.DownloadDirHistory),
                    TorrentFileAssociationStatuses.Normalize(Ui?.TorrentFileAssociation)));
        }

        public static SettingsFile FromDomain(AppSettings settings) =>
            new()
            {
                Daemon = new DaemonFile
                {
                    Host = settings.Daemon.Host,
                    Port = settings.Daemon.Port,
                    RpcPath = settings.Daemon.RpcPath,
                    Username = settings.Daemon.Username,
                    Password = settings.Daemon.Password
                },
                Ui = UiFile.FromDomain(settings.Ui)
            };
    }

    private sealed class DaemonFile
    {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 9091;
        public string RpcPath { get; set; } = "/transmission/rpc";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    private static string NormalizeColorScheme(string? value) =>
        !string.IsNullOrWhiteSpace(value) && UiColorSchemes.All.Contains(value)
            ? value
            : UiColorSchemes.Default;

    private static string NormalizeAppearance(string? value) =>
        !string.IsNullOrWhiteSpace(value) && UiAppearances.All.Contains(value)
            ? value
            : UiAppearances.Default;

    private static IReadOnlyList<string> NormalizeDownloadDirHistory(IReadOnlyList<string>? value) =>
        value?
            .Select(path => path.Trim())
            .Where(path => path.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .Take(DownloadDirHistoryHelper.MaxCount)
            .ToArray()
        ?? Array.Empty<string>();

    private sealed class UiFile
    {
        public int RefreshIntervalSeconds { get; set; } = 3;
        public int WindowWidth { get; set; } = 1280;
        public int WindowHeight { get; set; } = 800;
        public string? ColorScheme { get; set; }
        public string? Appearance { get; set; }
        public TorrentTableFile? TorrentTable { get; set; }
        public List<string>? DownloadDirHistory { get; set; }
        public string? TorrentFileAssociation { get; set; }

        public static UiFile FromDomain(UiSettings ui) =>
            new()
            {
                RefreshIntervalSeconds = ui.RefreshIntervalSeconds,
                WindowWidth = ui.WindowWidth,
                WindowHeight = ui.WindowHeight,
                ColorScheme = ui.ColorScheme,
                Appearance = ui.Appearance,
                TorrentTable = TorrentTableFile.FromDomain(ui.TorrentTable),
                DownloadDirHistory = ui.DownloadDirHistory?.ToList(),
                TorrentFileAssociation = ui.TorrentFileAssociation,
            };
    }

    private sealed class TorrentTableFile
    {
        public List<TorrentTableColumnFile>? Columns { get; set; }
        public string? SortColumnId { get; set; }
        public bool SortDescending { get; set; }

        public TorrentTableSettings ToDomain()
        {
            if (Columns is null || Columns.Count == 0)
                return TorrentTableSettings.CreateDefault();

            var columns = Columns
                .Where(c => TorrentTableColumnIds.All.Contains(c.Id))
                .Select(c => new TorrentTableColumnSetting(c.Id, c.Visible, c.WidthPx))
                .ToList();

            if (columns.Count == 0)
                return TorrentTableSettings.CreateDefault();

            var sortColumnId = TorrentTableColumnIds.All.Contains(SortColumnId ?? string.Empty)
                ? SortColumnId!
                : TorrentTableColumnIds.Name;

            return new TorrentTableSettings(columns, sortColumnId, SortDescending);
        }

        public static TorrentTableFile FromDomain(TorrentTableSettings settings) =>
            new()
            {
                Columns = settings.Columns
                    .Select(c => new TorrentTableColumnFile
                    {
                        Id = c.Id,
                        Visible = c.Visible,
                        WidthPx = c.WidthPx
                    })
                    .ToList(),
                SortColumnId = settings.SortColumnId,
                SortDescending = settings.SortDescending
            };
    }

    private sealed class TorrentTableColumnFile
    {
        public string Id { get; set; } = string.Empty;
        public bool Visible { get; set; }
        public int? WidthPx { get; set; }
    }
}
