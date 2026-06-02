namespace TransmissonNET.Infrastructure.Rpc;

internal sealed class RpcMethodNaming
{
    private int _rpcVersion = -1;

    public void SetRpcVersion(int rpcVersion) => _rpcVersion = rpcVersion;

    public string SessionGet => _rpcVersion >= 17 ? "session-get" : "session_get";

    public string TorrentGet => _rpcVersion >= 17 ? "torrent-get" : "torrent_get";

    public string SessionSet => _rpcVersion >= 17 ? "session-set" : "session_set";

    public string TorrentStart => _rpcVersion >= 17 ? "torrent-start" : "torrent_start";

    public string TorrentStop => _rpcVersion >= 17 ? "torrent-stop" : "torrent_stop";

    public string TorrentRemove => _rpcVersion >= 17 ? "torrent-remove" : "torrent_remove";

    public string TorrentVerify => _rpcVersion >= 17 ? "torrent-verify" : "torrent_verify";

    public string TorrentSet => _rpcVersion >= 17 ? "torrent-set" : "torrent_set";

    public string TorrentSetLocation => _rpcVersion >= 17 ? "torrent-set-location" : "torrent_set_location";

    public string TorrentRenamePath => _rpcVersion >= 17 ? "torrent-rename-path" : "torrent_rename_path";

    public string TorrentAdd => _rpcVersion >= 17 ? "torrent-add" : "torrent_add";
}
