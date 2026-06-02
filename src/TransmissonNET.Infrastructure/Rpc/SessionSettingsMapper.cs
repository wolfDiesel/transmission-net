using System.Text.Json;
using TransmissonNET.Domain;

namespace TransmissonNET.Infrastructure.Rpc;

internal static class SessionSettingsMapper
{
    public static readonly string[] FieldNames =
    [
        "download-dir",
        "incomplete-dir",
        "incomplete-dir-enabled",
        "trash-original-torrent-files",
        "peer-limit-global",
        "peer-limit-per-torrent",
        "speed-limit-down",
        "speed-limit-up",
        "speed-limit-down-enabled",
        "speed-limit-up-enabled",
        "seedRatioLimit",
        "seedRatioLimited",
        "idle-seeding-limit",
        "idle-seeding-limit-enabled",
    ];

    public static TransmissionDaemonSettings Map(JsonElement args) =>
        new(
            ReadString(args, "download-dir", "downloadDir"),
            ReadString(args, "incomplete-dir", "incompleteDir"),
            ReadBool(args, "incomplete-dir-enabled", "incompleteDirEnabled"),
            ReadBool(args, "trash-original-torrent-files", "trashOriginalTorrentFiles"),
            ReadInt32(args, "peer-limit-global", "peerLimitGlobal"),
            ReadInt32(args, "peer-limit-per-torrent", "peerLimitPerTorrent"),
            ReadInt32(args, "speed-limit-down", "speedLimitDown"),
            ReadInt32(args, "speed-limit-up", "speedLimitUp"),
            ReadBool(args, "speed-limit-down-enabled", "speedLimitDownEnabled"),
            ReadBool(args, "speed-limit-up-enabled", "speedLimitUpEnabled"),
            ReadDouble(args, "seedRatioLimit"),
            ReadBool(args, "seedRatioLimited"),
            ReadInt32(args, "idle-seeding-limit", "idleSeedingLimit"),
            ReadBool(args, "idle-seeding-limit-enabled", "idleSeedingLimitEnabled"));

    public static Dictionary<string, object> ToRpcArguments(TransmissionDaemonSettings settings) =>
        new()
        {
            ["download-dir"] = settings.DownloadDir,
            ["incomplete-dir"] = settings.IncompleteDir,
            ["incomplete-dir-enabled"] = settings.IncompleteDirEnabled,
            ["trash-original-torrent-files"] = settings.TrashOriginalTorrentFiles,
            ["peer-limit-global"] = settings.PeerLimitGlobal,
            ["peer-limit-per-torrent"] = settings.PeerLimitPerTorrent,
            ["speed-limit-down"] = settings.SpeedLimitDownKbps,
            ["speed-limit-up"] = settings.SpeedLimitUpKbps,
            ["speed-limit-down-enabled"] = settings.SpeedLimitDownEnabled,
            ["speed-limit-up-enabled"] = settings.SpeedLimitUpEnabled,
            ["seedRatioLimit"] = settings.SeedRatioLimit,
            ["seedRatioLimited"] = settings.SeedRatioLimited,
            ["idle-seeding-limit"] = settings.IdleSeedingLimitMinutes,
            ["idle-seeding-limit-enabled"] = settings.IdleSeedingLimitEnabled,
        };

    private static string ReadString(JsonElement args, string kebab, string camel) =>
        TryGet(args, kebab, out var kebabEl) || TryGet(args, camel, out kebabEl)
            ? kebabEl.GetString() ?? string.Empty
            : string.Empty;

    private static bool ReadBool(JsonElement args, string kebab, string? camel = null) =>
        TryGet(args, kebab, out var el) || (camel is not null && TryGet(args, camel, out el))
            ? el.ValueKind == JsonValueKind.True
            : false;

    private static int ReadInt32(JsonElement args, string kebab, string? camel = null) =>
        TryGet(args, kebab, out var el) || (camel is not null && TryGet(args, camel, out el))
            ? el.GetInt32()
            : 0;

    private static double ReadDouble(JsonElement args, string name) =>
        TryGet(args, name, out var el) ? el.GetDouble() : 0;

    private static bool TryGet(JsonElement args, string name, out JsonElement value)
    {
        if (args.TryGetProperty(name, out value))
            return true;

        value = default;
        return false;
    }
}
