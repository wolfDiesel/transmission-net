import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { useLocation } from 'react-router-dom'
import { useTorrents } from '../hooks/useTorrents'
import { useApp } from './AppProvider'

type TorrentListContextValue = ReturnType<typeof useTorrents> & {
  setTorrentPollingPaused: (paused: boolean) => void
}

const TorrentListContext = createContext<TorrentListContextValue | null>(null)

export function TorrentListProvider({ children }: { children: ReactNode }) {
  const { refreshIntervalSeconds } = useApp()
  const location = useLocation()
  const [pollingPaused, setPollingPaused] = useState(false)
  const onTorrentsPage = location.pathname === '/'
  const enabled = onTorrentsPage && !pollingPaused
  const torrentsState = useTorrents(refreshIntervalSeconds, enabled)

  const setTorrentPollingPaused = useCallback((paused: boolean) => {
    setPollingPaused(paused)
  }, [])

  const value = useMemo(
    () => ({ ...torrentsState, setTorrentPollingPaused }),
    [
      torrentsState.torrents,
      torrentsState.loading,
      torrentsState.refreshing,
      torrentsState.daemonConnected,
      torrentsState.lastUpdated,
      torrentsState.refreshNow,
      setTorrentPollingPaused,
    ],
  )

  return (
    <TorrentListContext.Provider value={value}>{children}</TorrentListContext.Provider>
  )
}

export function useTorrentList() {
  const ctx = useContext(TorrentListContext)
  if (!ctx) {
    throw new Error('useTorrentList must be used within TorrentListProvider')
  }
  return ctx
}

export function useTorrentListOptional() {
  return useContext(TorrentListContext)
}
