import type { TorrentDto } from '../../api/types'

export function deriveTorrentSpeeds(torrents: TorrentDto[]) {
  let downloadSpeed = 0
  let uploadSpeed = 0
  for (const torrent of torrents) {
    downloadSpeed += torrent.rateDownload
    uploadSpeed += torrent.rateUpload
  }
  return { downloadSpeed, uploadSpeed }
}
