using System.Text.Json;
using TransmissonNET.Domain;

namespace TransmissonNET.Infrastructure.Rpc;

internal static class TorrentAddMapper
{
    public static TorrentAddResult MapAddedTorrent(JsonElement arguments)
    {
        if (!TryGetAddedTorrent(arguments, out var added))
            throw new TransmissionRpcException("torrent-add did not return torrent-added.");

        var id = RpcJsonReader.GetInt32(added, "id");
        var name = RpcJsonReader.GetString(added, "name");
        var hash = RpcJsonReader.GetString(added, "hashString", "hash_string");

        return new TorrentAddResult(id, name, hash);
    }

    private static bool TryGetAddedTorrent(JsonElement arguments, out JsonElement added)
    {
        if (RpcJsonReader.TryGetProperty(arguments, "torrent-added", "torrent_added", out added))
            return true;

        if (RpcJsonReader.TryGetProperty(arguments, "torrent-duplicate", "torrent_duplicate", out added))
            return true;

        added = default;
        return false;
    }
}
