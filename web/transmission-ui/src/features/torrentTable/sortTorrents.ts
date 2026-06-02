import type { TorrentDto } from '../../api/types'
import type { TorrentColumnId } from './columnIds'
import { getTorrentSortValue } from './columns'

export function sortTorrents(
  torrents: TorrentDto[],
  columnId: string,
  descending: boolean,
): TorrentDto[] {
  const sorted = [...torrents]
  sorted.sort((a, b) => {
    const av = getTorrentSortValue(a, columnId as TorrentColumnId)
    const bv = getTorrentSortValue(b, columnId as TorrentColumnId)
    if (typeof av === 'number' && typeof bv === 'number') {
      return descending ? bv - av : av - bv
    }
    const cmp = String(av).localeCompare(String(bv))
    return descending ? -cmp : cmp
  })
  return sorted
}
