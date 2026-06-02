import type { ContextMenuItem } from '../ui/ContextMenu'
import type { TorrentDto, TorrentBandwidthPriority } from '../../api/types'
import { priorityLabel } from '../../api/torrentActions'

const PRIORITIES: TorrentBandwidthPriority[] = ['high', 'normal', 'low']

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
): ContextMenuItem[] {
  return [
    { type: 'item', id: 'start', label: 'Start', onSelect: handlers.onStart },
    { type: 'item', id: 'stop', label: 'Stop', onSelect: handlers.onStop },
    { type: 'separator', id: 'sep-actions' },
    {
      type: 'submenu',
      id: 'priority',
      label: 'Priority',
      items: PRIORITIES.map((priority) => ({
        type: 'item' as const,
        id: `priority-${priority}`,
        label:
          torrent.bandwidthPriority === priority
            ? `✓ ${priorityLabel(priority)}`
            : priorityLabel(priority),
        onSelect: () => handlers.onSetPriority(priority),
      })),
    },
    { type: 'separator', id: 'sep-priority' },
    { type: 'item', id: 'move', label: 'Move…', onSelect: handlers.onMove },
    { type: 'item', id: 'verify', label: 'Verify', onSelect: handlers.onVerify },
    { type: 'separator', id: 'sep-danger' },
    {
      type: 'item',
      id: 'remove',
      label: 'Remove…',
      tone: 'danger',
      onSelect: handlers.onRemove,
    },
  ]
}
