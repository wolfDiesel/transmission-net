export const TorrentColumnId = {
  Name: 'name',
  Progress: 'progress',
  AddedDate: 'addedDate',
  DoneDate: 'doneDate',
  Status: 'status',
  Size: 'size',
  DownloadSpeed: 'downloadSpeed',
  UploadSpeed: 'uploadSpeed',
  Eta: 'eta',
  UploadRatio: 'uploadRatio',
  Peers: 'peers',
  Downloaded: 'downloaded',
  Uploaded: 'uploaded',
  Queue: 'queue',
  DownloadDir: 'downloadDir',
  Left: 'left',
} as const

export type TorrentColumnId = (typeof TorrentColumnId)[keyof typeof TorrentColumnId]

export const ALL_TORRENT_COLUMN_IDS: TorrentColumnId[] = [
  TorrentColumnId.Name,
  TorrentColumnId.Progress,
  TorrentColumnId.AddedDate,
  TorrentColumnId.DoneDate,
  TorrentColumnId.Status,
  TorrentColumnId.Size,
  TorrentColumnId.DownloadSpeed,
  TorrentColumnId.UploadSpeed,
  TorrentColumnId.Eta,
  TorrentColumnId.UploadRatio,
  TorrentColumnId.Peers,
  TorrentColumnId.Downloaded,
  TorrentColumnId.Uploaded,
  TorrentColumnId.Queue,
  TorrentColumnId.DownloadDir,
  TorrentColumnId.Left,
]
