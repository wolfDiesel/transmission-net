import { Table } from '@chakra-ui/react'
import { memo } from 'react'
import type { TorrentDto } from '../../api/types'
import { TorrentColumnId } from '../../features/torrentTable/columnIds'
import {
  formatBytesPerSec,
  formatEta,
  formatRatio,
  formatSize,
  formatUnixDate,
  statusLabel,
} from '../../utils/format'
import { ProgressCell } from './ProgressCell'

type TorrentTableCellProps = {
  columnId: string
  torrent: TorrentDto
  widthPx: number
}

const cellWidth = (widthPx: number) => ({
  w: `${widthPx}px`,
  minW: `${widthPx}px`,
  maxW: `${widthPx}px`,
  overflow: 'hidden',
  textOverflow: 'ellipsis',
  whiteSpace: 'nowrap' as const,
})

export const TorrentTableCell = memo(function TorrentTableCell({
  columnId,
  torrent,
  widthPx,
}: TorrentTableCellProps) {
  switch (columnId) {
    case TorrentColumnId.Name:
      return (
        <Table.Cell {...cellWidth(widthPx)} title={torrent.name}>
          {torrent.name}
        </Table.Cell>
      )
    case TorrentColumnId.Progress:
      return (
        <Table.Cell {...cellWidth(widthPx)} whiteSpace="normal">
          <ProgressCell percentDone={torrent.percentDone} />
        </Table.Cell>
      )
    case TorrentColumnId.AddedDate:
      return <Table.Cell {...cellWidth(widthPx)}>{formatUnixDate(torrent.addedDate)}</Table.Cell>
    case TorrentColumnId.DoneDate:
      return <Table.Cell {...cellWidth(widthPx)}>{formatUnixDate(torrent.doneDate)}</Table.Cell>
    case TorrentColumnId.Status:
      return <Table.Cell {...cellWidth(widthPx)}>{statusLabel(torrent.status)}</Table.Cell>
    case TorrentColumnId.Size:
      return <Table.Cell {...cellWidth(widthPx)} textAlign="end">{formatSize(torrent.totalSize)}</Table.Cell>
    case TorrentColumnId.DownloadSpeed:
      return <Table.Cell {...cellWidth(widthPx)} textAlign="end">{formatBytesPerSec(torrent.rateDownload)}</Table.Cell>
    case TorrentColumnId.UploadSpeed:
      return <Table.Cell {...cellWidth(widthPx)} textAlign="end">{formatBytesPerSec(torrent.rateUpload)}</Table.Cell>
    case TorrentColumnId.Eta:
      return <Table.Cell {...cellWidth(widthPx)} textAlign="end">{formatEta(torrent.eta)}</Table.Cell>
    case TorrentColumnId.UploadRatio:
      return <Table.Cell {...cellWidth(widthPx)} textAlign="end">{formatRatio(torrent.uploadRatio)}</Table.Cell>
    case TorrentColumnId.Peers:
      return <Table.Cell {...cellWidth(widthPx)} textAlign="end">{torrent.peersConnected}</Table.Cell>
    case TorrentColumnId.Downloaded:
      return <Table.Cell {...cellWidth(widthPx)} textAlign="end">{formatSize(torrent.downloadedEver)}</Table.Cell>
    case TorrentColumnId.Uploaded:
      return <Table.Cell {...cellWidth(widthPx)} textAlign="end">{formatSize(torrent.uploadedEver)}</Table.Cell>
    case TorrentColumnId.Queue:
      return <Table.Cell {...cellWidth(widthPx)} textAlign="end">{torrent.queuePosition}</Table.Cell>
    case TorrentColumnId.DownloadDir:
      return (
        <Table.Cell {...cellWidth(widthPx)} title={torrent.downloadDir}>
          {torrent.downloadDir || '—'}
        </Table.Cell>
      )
    case TorrentColumnId.Left:
      return <Table.Cell {...cellWidth(widthPx)} textAlign="end">{formatSize(torrent.leftUntilDone)}</Table.Cell>
    default:
      return <Table.Cell {...cellWidth(widthPx)}>—</Table.Cell>
  }
})
