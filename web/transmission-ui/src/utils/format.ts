import { TorrentStatus } from '../api/types'

export function formatBytesPerSec(bytes: number): string {
  if (bytes <= 0) return '—'
  const units = ['B/s', 'KB/s', 'MB/s', 'GB/s']
  let value = bytes
  let unit = 0
  while (value >= 1024 && unit < units.length - 1) {
    value /= 1024
    unit++
  }
  return `${value.toFixed(unit === 0 ? 0 : 1)} ${units[unit]}`
}

export function formatEta(seconds: number): string {
  if (seconds < 0) return '—'
  if (seconds === 0) return '0s'
  const h = Math.floor(seconds / 3600)
  const m = Math.floor((seconds % 3600) / 60)
  const s = seconds % 60
  if (h > 0) return `${h}h ${m}m`
  if (m > 0) return `${m}m ${s}s`
  return `${s}s`
}

export function formatPercent(value: number): string {
  return `${(value * 100).toFixed(1)}%`
}

export function formatSize(bytes: number): string {
  if (bytes <= 0) return '—'
  const units = ['B', 'KB', 'MB', 'GB', 'TB']
  let value = bytes
  let unit = 0
  while (value >= 1024 && unit < units.length - 1) {
    value /= 1024
    unit++
  }
  return `${value.toFixed(1)} ${units[unit]}`
}

export function statusLabel(status: TorrentStatus): string {
  switch (status) {
    case TorrentStatus.Stopped:
      return 'Stopped'
    case TorrentStatus.CheckWait:
      return 'Queued (check)'
    case TorrentStatus.Checking:
      return 'Checking'
    case TorrentStatus.DownloadWait:
      return 'Queued'
    case TorrentStatus.Downloading:
      return 'Downloading'
    case TorrentStatus.SeedWait:
      return 'Queued (seed)'
    case TorrentStatus.Seeding:
      return 'Seeding'
    default:
      return 'Unknown'
  }
}

export function formatUnixDate(seconds: number): string {
  if (seconds <= 0) return '—'
  return new Date(seconds * 1000).toLocaleString()
}

export function formatRatio(value: number): string {
  if (value < 0) return '—'
  return value.toFixed(2)
}

export function formatLastUpdated(date: Date | null): string {
  if (!date) return 'never'
  const seconds = Math.floor((Date.now() - date.getTime()) / 1000)
  if (seconds < 5) return 'just now'
  if (seconds < 60) return `${seconds}s ago`
  return date.toLocaleTimeString()
}
