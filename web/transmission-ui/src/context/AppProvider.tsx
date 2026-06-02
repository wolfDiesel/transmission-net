import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { api, ApiError } from '../api/client'
import type { AppSettingsDto } from '../api/types'
import { createDefaultTorrentTableSettings } from '../features/torrentTable/defaults'
import { normalizeTorrentTableSettings } from '../features/torrentTable/normalizeTableSettings'
import { DEFAULT_APPEARANCE, DEFAULT_COLOR_SCHEME, normalizeAppearance, normalizeColorScheme } from '../theme/accentPalettes'

const defaultSettings: AppSettingsDto = {
  daemon: {
    host: '127.0.0.1',
    port: 9091,
    rpcPath: '/transmission/rpc',
    username: '',
    password: '',
  },
  ui: {
    refreshIntervalSeconds: 3,
    windowWidth: 1280,
    windowHeight: 800,
    torrentTable: createDefaultTorrentTableSettings(),
    colorScheme: DEFAULT_COLOR_SCHEME,
    appearance: DEFAULT_APPEARANCE,
    downloadDirHistory: [],
  },
}

type AppContextValue = {
  settings: AppSettingsDto
  setSettings: React.Dispatch<React.SetStateAction<AppSettingsDto>>
  settingsLoading: boolean
  settingsError: string | null
  applySavedSettings: (saved: AppSettingsDto, keepPassword: string) => void
  refreshIntervalSeconds: number
}

const AppContext = createContext<AppContextValue | null>(null)

export function AppProvider({ children }: { children: ReactNode }) {
  const [settings, setSettings] = useState<AppSettingsDto>(defaultSettings)
  const [settingsLoading, setSettingsLoading] = useState(true)
  const [settingsError, setSettingsError] = useState<string | null>(null)

  const refreshIntervalSeconds = Math.max(settings.ui.refreshIntervalSeconds, 1)

  const loadSettings = useCallback(async () => {
    try {
      setSettingsError(null)
      const data = await api.getSettings()
      setSettings({
        ...data,
        daemon: { ...data.daemon, password: data.daemon.password ?? '' },
        ui: {
          ...data.ui,
          torrentTable: normalizeTorrentTableSettings(data.ui.torrentTable),
          colorScheme: normalizeColorScheme(data.ui.colorScheme),
          appearance: normalizeAppearance(data.ui.appearance),
          downloadDirHistory: data.ui.downloadDirHistory ?? [],
        },
      })
    } catch (e) {
      setSettingsError(e instanceof ApiError ? e.message : 'Failed to load settings')
    } finally {
      setSettingsLoading(false)
    }
  }, [])

  const applySavedSettings = useCallback((saved: AppSettingsDto, keepPassword: string) => {
    setSettings({
      ...saved,
      daemon: { ...saved.daemon, password: keepPassword },
    })
  }, [])

  useEffect(() => {
    void loadSettings()
  }, [loadSettings])

  const value = useMemo(
    () => ({
      settings,
      setSettings,
      settingsLoading,
      settingsError,
      applySavedSettings,
      refreshIntervalSeconds,
    }),
    [settings, settingsLoading, settingsError, applySavedSettings, refreshIntervalSeconds],
  )

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>
}

export function useApp() {
  const ctx = useContext(AppContext)
  if (!ctx) throw new Error('useApp must be used within AppProvider')
  return ctx
}
