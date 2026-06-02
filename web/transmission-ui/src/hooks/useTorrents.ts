import { startTransition, useCallback, useEffect, useRef, useState } from 'react'
import { api } from '../api/client'
import { mergeTorrentList } from '../features/torrentList/mergeTorrentList'
import type { TorrentDto } from '../api/types'

export function useTorrents(refreshIntervalSeconds: number, enabled: boolean) {
  const [torrents, setTorrents] = useState<TorrentDto[]>([])
  const [initialLoading, setInitialLoading] = useState(true)
  const [refreshing, setRefreshing] = useState(false)
  const [daemonConnected, setDaemonConnected] = useState(false)
  const [lastUpdated, setLastUpdated] = useState<Date | null>(null)
  const inFlightRef = useRef(false)

  const applyTorrents = useCallback((data: TorrentDto[]) => {
    startTransition(() => {
      setTorrents((prev) => mergeTorrentList(prev, data))
    })
  }, [])

  const refreshNow = useCallback(
    async (silent = false) => {
      if (!enabled || inFlightRef.current) return

      inFlightRef.current = true
      if (!silent) setRefreshing(true)

      try {
        const data = await api.getTorrents()
        setDaemonConnected(true)
        applyTorrents(data)
        if (!silent) setLastUpdated(new Date())
      } catch {
        setDaemonConnected(false)
        if (!silent) {
          startTransition(() => setTorrents([]))
        }
      } finally {
        inFlightRef.current = false
        setInitialLoading(false)
        if (!silent) setRefreshing(false)
      }
    },
    [applyTorrents, enabled],
  )

  useEffect(() => {
    if (!enabled) return

    void refreshNow(false)
    const intervalMs = Math.max(refreshIntervalSeconds, 1) * 1000
    const timer = window.setInterval(() => void refreshNow(true), intervalMs)
    return () => window.clearInterval(timer)
  }, [enabled, refreshNow, refreshIntervalSeconds])

  return {
    torrents,
    loading: initialLoading,
    refreshing,
    daemonConnected,
    lastUpdated,
    refreshNow: () => refreshNow(false),
  }
}
