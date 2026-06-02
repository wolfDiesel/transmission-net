import type { TorrentDto } from '../../api/types'
import { TorrentColumnId } from './columnIds'

export type TorrentColumnDef = {
  id: TorrentColumnId
  label: string
  align?: 'start' | 'end'
  minW?: string
  sortable?: boolean
}

export const TORRENT_COLUMN_DEFS: Record<TorrentColumnId, TorrentColumnDef> = {
  [TorrentColumnId.Name]: { id: TorrentColumnId.Name, label: 'Name', minW: '220px' },
  [TorrentColumnId.Progress]: {
    id: TorrentColumnId.Progress,
    label: 'Progress',
    minW: '140px',
    sortable: true,
  },
  [TorrentColumnId.AddedDate]: {
    id: TorrentColumnId.AddedDate,
    label: 'Added',
    minW: '150px',
    sortable: true,
  },
  [TorrentColumnId.DoneDate]: {
    id: TorrentColumnId.DoneDate,
    label: 'Finished',
    minW: '150px',
    sortable: true,
  },
  [TorrentColumnId.Status]: { id: TorrentColumnId.Status, label: 'Status', sortable: true },
  [TorrentColumnId.Size]: {
    id: TorrentColumnId.Size,
    label: 'Size',
    align: 'end',
    sortable: true,
  },
  [TorrentColumnId.DownloadSpeed]: {
    id: TorrentColumnId.DownloadSpeed,
    label: 'Down',
    align: 'end',
    sortable: true,
  },
  [TorrentColumnId.UploadSpeed]: {
    id: TorrentColumnId.UploadSpeed,
    label: 'Up',
    align: 'end',
    sortable: true,
  },
  [TorrentColumnId.Eta]: { id: TorrentColumnId.Eta, label: 'ETA', align: 'end', sortable: true },
  [TorrentColumnId.UploadRatio]: {
    id: TorrentColumnId.UploadRatio,
    label: 'Ratio',
    align: 'end',
    sortable: true,
  },
  [TorrentColumnId.Peers]: {
    id: TorrentColumnId.Peers,
    label: 'Peers',
    align: 'end',
    sortable: true,
  },
  [TorrentColumnId.Downloaded]: {
    id: TorrentColumnId.Downloaded,
    label: 'Downloaded',
    align: 'end',
    sortable: true,
  },
  [TorrentColumnId.Uploaded]: {
    id: TorrentColumnId.Uploaded,
    label: 'Uploaded',
    align: 'end',
    sortable: true,
  },
  [TorrentColumnId.Queue]: {
    id: TorrentColumnId.Queue,
    label: 'Queue',
    align: 'end',
    sortable: true,
  },
  [TorrentColumnId.DownloadDir]: {
    id: TorrentColumnId.DownloadDir,
    label: 'Folder',
    minW: '180px',
  },
  [TorrentColumnId.Left]: {
    id: TorrentColumnId.Left,
    label: 'Left',
    align: 'end',
    sortable: true,
  },
}

export function getColumnDef(id: string): TorrentColumnDef | undefined {
  return TORRENT_COLUMN_DEFS[id as TorrentColumnId]
}

export type TorrentSortValue = string | number

export function getTorrentSortValue(torrent: TorrentDto, columnId: TorrentColumnId): TorrentSortValue {
  switch (columnId) {
    case TorrentColumnId.Name:
      return torrent.name.toLowerCase()
    case TorrentColumnId.Progress:
      return torrent.percentDone
    case TorrentColumnId.AddedDate:
      return torrent.addedDate
    case TorrentColumnId.DoneDate:
      return torrent.doneDate
    case TorrentColumnId.Status:
      return torrent.status
    case TorrentColumnId.Size:
      return torrent.totalSize
    case TorrentColumnId.DownloadSpeed:
      return torrent.rateDownload
    case TorrentColumnId.UploadSpeed:
      return torrent.rateUpload
    case TorrentColumnId.Eta:
      return torrent.eta
    case TorrentColumnId.UploadRatio:
      return torrent.uploadRatio
    case TorrentColumnId.Peers:
      return torrent.peersConnected
    case TorrentColumnId.Downloaded:
      return torrent.downloadedEver
    case TorrentColumnId.Uploaded:
      return torrent.uploadedEver
    case TorrentColumnId.Queue:
      return torrent.queuePosition
    case TorrentColumnId.DownloadDir:
      return torrent.downloadDir.toLowerCase()
    case TorrentColumnId.Left:
      return torrent.leftUntilDone
    default:
      return ''
  }
}
