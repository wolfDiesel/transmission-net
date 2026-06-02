import type { TorrentTableSettingsDto } from '../../api/types'
import { ALL_TORRENT_COLUMN_IDS } from './columnIds'
import { clampColumnWidth } from './columnWidths'
import { createDefaultTorrentTableSettings } from './defaults'

export function normalizeTorrentTableSettings(
  input: TorrentTableSettingsDto | undefined | null,
): TorrentTableSettingsDto {
  const defaults = createDefaultTorrentTableSettings()
  if (!input?.columns?.length) return defaults

  const defaultVisibility = new Map(defaults.columns.map((c) => [c.id, c.visible]))
  const inputVisibility = new Map(
    input.columns
      .filter((c) =>
        ALL_TORRENT_COLUMN_IDS.includes(c.id as (typeof ALL_TORRENT_COLUMN_IDS)[number]),
      )
      .map((c) => [c.id, c.visible]),
  )

  const orderFromInput = input.columns
    .map((c) => c.id)
    .filter((id) => ALL_TORRENT_COLUMN_IDS.includes(id as (typeof ALL_TORRENT_COLUMN_IDS)[number]))

  const orderedIds = [...orderFromInput]
  for (const id of ALL_TORRENT_COLUMN_IDS) {
    if (!orderedIds.includes(id)) orderedIds.push(id)
  }

  const inputWidths = new Map(
    input.columns
      .filter((c) =>
        ALL_TORRENT_COLUMN_IDS.includes(c.id as (typeof ALL_TORRENT_COLUMN_IDS)[number]),
      )
      .map((c) => [c.id, c.widthPx] as const),
  )

  const orderedColumns = orderedIds.map((id) => {
    const widthPx = inputWidths.get(id)
    return {
      id,
      visible: inputVisibility.has(id) ? inputVisibility.get(id)! : (defaultVisibility.get(id) ?? false),
      ...(widthPx !== undefined && widthPx !== null ? { widthPx: clampColumnWidth(widthPx) } : {}),
    }
  })

  const sortColumnId = ALL_TORRENT_COLUMN_IDS.includes(
    input.sortColumnId as (typeof ALL_TORRENT_COLUMN_IDS)[number],
  )
    ? input.sortColumnId
    : defaults.sortColumnId

  return {
    columns: orderedColumns,
    sortColumnId,
    sortDescending: input.sortDescending,
  }
}

export function getVisibleColumnIds(settings: TorrentTableSettingsDto): string[] {
  return settings.columns.filter((c) => c.visible).map((c) => c.id)
}
