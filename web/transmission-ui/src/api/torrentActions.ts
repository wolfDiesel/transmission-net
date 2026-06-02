import type { TorrentBandwidthPriority, TorrentActionRequest } from './types'
import { api } from './client'

export { ApiError } from './client'

export async function executeTorrentAction(request: TorrentActionRequest): Promise<void> {
  await api.executeTorrentAction(request)
}

export function priorityLabel(priority: TorrentBandwidthPriority): string {
  switch (priority) {
    case 'high':
      return 'High'
    case 'low':
      return 'Low'
    default:
      return 'Normal'
  }
}
