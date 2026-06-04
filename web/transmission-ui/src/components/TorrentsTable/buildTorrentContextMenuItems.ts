import type { TranslateFn } from '../../i18n'
import type { ContextMenuItem } from '../ui/ContextMenu'
import type { TorrentDto, TorrentBandwidthPriority } from '../../api/types'

const PRIORITIES: TorrentBandwidthPriority[] = ['high', 'normal', 'low']

const PRIORITY_KEYS: Record<TorrentBandwidthPriority, string> = {
  high: 'torrentTable.contextMenu.high',
  normal: 'torrentTable.contextMenu.normal',
  low: 'torrentTable.contextMenu.low',
}

export type TorrentContextMenuHandlers = {
  onStart: () => void
  onStop: () => void
  onVerify: () => void
  onMove: () => void
  onRemove: () => void
  onSetPriority: (priority: TorrentBandwidthPriority) => void
}

export function buildTorrentContextMenuItems(
  torrent: TorrentDto,
  handlers: TorrentContextMenuHandlers,
  t: TranslateFn,
): ContextMenuItem[] {
  return [
    { type: 'item', id: 'start', label: t('torrentTable.contextMenu.start'), onSelect: handlers.onStart },
    { type: 'item', id: 'stop', label: t('torrentTable.contextMenu.stop'), onSelect: handlers.onStop },
    { type: 'separator', id: 'sep-actions' },
    {
      type: 'submenu',
      id: 'priority',
      label: t('torrentTable.contextMenu.priority'),
      items: PRIORITIES.map((priority) => {
        const label = t(PRIORITY_KEYS[priority])
        return {
          type: 'item' as const,
          id: `priority-${priority}`,
          label: torrent.bandwidthPriority === priority ? `✓ ${label}` : label,
          onSelect: () => handlers.onSetPriority(priority),
        }
      }),
    },
    { type: 'separator', id: 'sep-priority' },
    { type: 'item', id: 'move', label: t('torrentTable.contextMenu.move'), onSelect: handlers.onMove },
    { type: 'item', id: 'verify', label: t('torrentTable.contextMenu.verify'), onSelect: handlers.onVerify },
    { type: 'separator', id: 'sep-danger' },
    {
      type: 'item',
      id: 'remove',
      label: t('torrentTable.contextMenu.remove'),
      tone: 'danger',
      onSelect: handlers.onRemove,
    },
  ]
}
