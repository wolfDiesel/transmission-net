import type { TorrentTableSettingsDto } from '../../api/types'
import { TorrentColumnId, type TorrentColumnId as TorrentColumnIdType } from './columnIds'

export const MIN_COLUMN_WIDTH_PX = 56
export const MAX_COLUMN_WIDTH_PX = 640

export const DEFAULT_COLUMN_WIDTH_PX: Record<TorrentColumnIdType, number> = {
  [TorrentColumnId.Name]: 280,
  [TorrentColumnId.Progress]: 150,
  [TorrentColumnId.AddedDate]: 160,
  [TorrentColumnId.DoneDate]: 160,
  [TorrentColumnId.Status]: 120,
  [TorrentColumnId.Size]: 100,
  [TorrentColumnId.DownloadSpeed]: 100,
  [TorrentColumnId.UploadSpeed]: 100,
  [TorrentColumnId.Eta]: 90,
  [TorrentColumnId.UploadRatio]: 80,
  [TorrentColumnId.Peers]: 72,
  [TorrentColumnId.Downloaded]: 110,
  [TorrentColumnId.Uploaded]: 110,
  [TorrentColumnId.Queue]: 72,
  [TorrentColumnId.DownloadDir]: 200,
  [TorrentColumnId.Left]: 100,
}

export function clampColumnWidth(widthPx: number): number {
  return Math.round(Math.min(MAX_COLUMN_WIDTH_PX, Math.max(MIN_COLUMN_WIDTH_PX, widthPx)))
}

export function getDefaultColumnWidthPx(columnId: string): number {
  return DEFAULT_COLUMN_WIDTH_PX[columnId as TorrentColumnIdType] ?? 120
}

export function getColumnWidthPx(
  settings: TorrentTableSettingsDto,
  columnId: string,
): number {
  const saved = settings.columns.find((c) => c.id === columnId)?.widthPx
  if (saved !== undefined && saved !== null) {
    return clampColumnWidth(saved)
  }
  return getDefaultColumnWidthPx(columnId)
}

export function buildColumnWidthMap(
  settings: TorrentTableSettingsDto,
  columnIds: string[],
  overrides?: Record<string, number>,
): Record<string, number> {
  const map: Record<string, number> = {}
  for (const id of columnIds) {
    map[id] = overrides?.[id] ?? getColumnWidthPx(settings, id)
  }
  return map
}
