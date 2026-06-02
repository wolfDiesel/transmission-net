import type { TorrentTableSettingsDto } from '../../api/types'
import { TorrentColumnId } from './columnIds'

export function createDefaultTorrentTableSettings(): TorrentTableSettingsDto {
  return {
    columns: [
      { id: TorrentColumnId.Name, visible: true },
      { id: TorrentColumnId.Progress, visible: true },
      { id: TorrentColumnId.AddedDate, visible: true },
      { id: TorrentColumnId.Status, visible: true },
      { id: TorrentColumnId.Size, visible: true },
      { id: TorrentColumnId.DownloadSpeed, visible: true },
      { id: TorrentColumnId.UploadSpeed, visible: true },
      { id: TorrentColumnId.Eta, visible: true },
      { id: TorrentColumnId.DoneDate, visible: false },
      { id: TorrentColumnId.UploadRatio, visible: false },
      { id: TorrentColumnId.Peers, visible: false },
      { id: TorrentColumnId.Downloaded, visible: false },
      { id: TorrentColumnId.Uploaded, visible: false },
      { id: TorrentColumnId.Queue, visible: false },
      { id: TorrentColumnId.DownloadDir, visible: false },
      { id: TorrentColumnId.Left, visible: false },
    ],
    sortColumnId: TorrentColumnId.Name,
    sortDescending: false,
  }
}
