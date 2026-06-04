import { useCallback } from 'react'
import { useI18n } from '../i18n'
import {
  TORRENT_COLUMN_DEFS,
  type TorrentColumnDef,
} from '../features/torrentTable/columns'
import type { TorrentColumnId } from '../features/torrentTable/columnIds'

export function useTorrentColumnDef() {
  const { t } = useI18n()

  return useCallback(
    (id: string): TorrentColumnDef | undefined => {
      const meta = TORRENT_COLUMN_DEFS[id as TorrentColumnId]
      if (!meta) return undefined
      return { ...meta, label: t(`torrentTable.columns.${id}`) }
    },
    [t],
  )
}
