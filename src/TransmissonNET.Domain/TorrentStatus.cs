namespace TransmissonNET.Domain;

public enum TorrentStatus
{
    Stopped = 0,
    CheckWait = 1,
    Checking = 2,
    DownloadWait = 3,
    Downloading = 4,
    SeedWait = 5,
    Seeding = 6,
    Unknown = -1
}
