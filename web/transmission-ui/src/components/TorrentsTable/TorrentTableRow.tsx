import { Table } from '@chakra-ui/react'
import { memo, type MouseEvent } from 'react'
import type { TorrentDto } from '../../api/types'
import { TorrentTableCell } from './TorrentTableCell'
import { getTorrentTableRowProps } from './torrentTableRowStyles'

type TorrentTableRowProps = {
  torrent: TorrentDto
  selectedId: number | null
  menuRowId: number | null
  visibleColumnIds: string[]
  columnWidths: Record<string, number>
  onSelect: (torrent: TorrentDto) => void
  onContextMenu: (torrent: TorrentDto, event: MouseEvent) => void
}

export const TorrentTableRow = memo(function TorrentTableRow({
  torrent,
  selectedId,
  menuRowId,
  visibleColumnIds,
  columnWidths,
  onSelect,
  onContextMenu,
}: TorrentTableRowProps) {
  return (
    <Table.Row
      {...getTorrentTableRowProps(torrent.id, selectedId, menuRowId)}
      onClick={() => onSelect(torrent)}
      onContextMenu={(event) => onContextMenu(torrent, event)}
    >
      {visibleColumnIds.map((columnId) => (
        <TorrentTableCell
          key={columnId}
          columnId={columnId}
          torrent={torrent}
          widthPx={columnWidths[columnId]}
        />
      ))}
    </Table.Row>
  )
})
