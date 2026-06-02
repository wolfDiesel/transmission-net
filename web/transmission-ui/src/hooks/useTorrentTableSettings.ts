import { useCallback, useMemo, useRef } from 'react'
import { api } from '../api/client'
import type { TorrentTableSettingsDto } from '../api/types'
import { useApp } from '../context/AppProvider'
import { clampColumnWidth } from '../features/torrentTable/columnWidths'
import { getVisibleColumnIds, normalizeTorrentTableSettings } from '../features/torrentTable/normalizeTableSettings'

export function useTorrentTableSettings() {
  const { settings, setSettings, applySavedSettings } = useApp()
  const saveTimer = useRef<number | undefined>(undefined)

  const tableSettings = useMemo(
    () => normalizeTorrentTableSettings(settings.ui.torrentTable),
    [settings.ui.torrentTable],
  )

  const visibleColumnIds = useMemo(
    () => getVisibleColumnIds(tableSettings),
    [tableSettings],
  )

  const persist = useCallback(
    (next: TorrentTableSettingsDto) => {
      const normalized = normalizeTorrentTableSettings(next)
      setSettings((prev) => ({
        ...prev,
        ui: { ...prev.ui, torrentTable: normalized },
      }))

      if (saveTimer.current !== undefined) {
        window.clearTimeout(saveTimer.current)
      }

      saveTimer.current = window.setTimeout(() => {
        void (async () => {
          const password = settings.daemon.password ?? ''
          const saved = await api.saveSettings({
            ...settings,
            ui: { ...settings.ui, torrentTable: normalized },
            daemon: {
              ...settings.daemon,
              password: password || null,
            },
          })
          applySavedSettings(saved, password)
        })()
      }, 400)
    },
    [applySavedSettings, setSettings, settings],
  )

  const setSort = useCallback(
    (columnId: string) => {
      const descending =
        tableSettings.sortColumnId === columnId ? !tableSettings.sortDescending : false
      persist({
        ...tableSettings,
        sortColumnId: columnId,
        sortDescending: descending,
      })
    },
    [persist, tableSettings],
  )

  const setColumns = useCallback(
    (columns: TorrentTableSettingsDto['columns']) => {
      persist({ ...tableSettings, columns })
    },
    [persist, tableSettings],
  )

  const setColumnWidth = useCallback(
    (columnId: string, widthPx: number) => {
      const width = clampColumnWidth(widthPx)
      const columns = tableSettings.columns.map((c) =>
        c.id === columnId ? { ...c, widthPx: width } : c,
      )
      persist({ ...tableSettings, columns })
    },
    [persist, tableSettings],
  )

  return {
    tableSettings,
    visibleColumnIds,
    setSort,
    setColumns,
    setColumnWidth,
  }
}
