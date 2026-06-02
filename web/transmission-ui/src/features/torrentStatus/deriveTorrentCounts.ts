import { TorrentStatus, type TorrentDto } from '../../api/types'

export function deriveTorrentCounts(torrents: TorrentDto[]) {
  let downloading = 0
  let completed = 0

  for (const torrent of torrents) {
    if (
      torrent.status === TorrentStatus.Downloading ||
      torrent.status === TorrentStatus.DownloadWait
    ) {
      downloading++
    }
    if (torrent.percentDone >= 1 || torrent.status === TorrentStatus.Seeding) {
      completed++
    }
  }

  return { downloading, completed }
}
