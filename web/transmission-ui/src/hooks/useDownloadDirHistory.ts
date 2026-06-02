import { useCallback, useMemo, useRef } from 'react'
import { api } from '../api/client'
import { useApp } from '../context/AppProvider'
import { pushDownloadDirHistory } from '../features/downloadDir/downloadDirHistory'

export function useDownloadDirHistory() {
  const { settings, setSettings, applySavedSettings } = useApp()
  const saveTimer = useRef<number | undefined>(undefined)

  const directories = useMemo(
    () => settings.ui.downloadDirHistory ?? [],
    [settings.ui.downloadDirHistory],
  )

  const remember = useCallback(
    (path: string) => {
      const trimmed = path.trim()
      if (!trimmed || directories[0] === trimmed) return

      const next = pushDownloadDirHistory(directories, trimmed)

      setSettings((prev) => ({
        ...prev,
        ui: { ...prev.ui, downloadDirHistory: next },
      }))

      if (saveTimer.current !== undefined) {
        window.clearTimeout(saveTimer.current)
      }

      saveTimer.current = window.setTimeout(() => {
        void (async () => {
          const password = settings.daemon.password ?? ''
          const saved = await api.saveSettings({
            ...settings,
            ui: { ...settings.ui, downloadDirHistory: next },
            daemon: {
              ...settings.daemon,
              password: password || null,
            },
          })
          applySavedSettings(saved, password)
        })()
      }, 400)
    },
    [applySavedSettings, directories, setSettings, settings],
  )

  return { directories, remember }
}
