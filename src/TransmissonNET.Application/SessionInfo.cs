namespace TransmissonNET.Application;

public sealed record SessionInfo(
    int RpcVersion,
    string Version,
    long DownloadSpeed,
    long UploadSpeed);
