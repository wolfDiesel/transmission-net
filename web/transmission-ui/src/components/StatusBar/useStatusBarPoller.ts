import { startTransition, useCallback, useEffect, useRef, useState } from 'react'
import { api } from '../../api/client'
import type { DaemonStatusDto } from '../../api/types'

const disconnected: DaemonStatusDto = {
  connected: false,
  downloadSpeed: 0,
  uploadSpeed: 0,
  downloadingCount: 0,
  completedCount: 0,
}

function statusEquals(a: DaemonStatusDto, b: DaemonStatusDto) {
  return (
    a.connected === b.connected &&
    a.downloadSpeed === b.downloadSpeed &&
    a.uploadSpeed === b.uploadSpeed &&
    a.downloadingCount === b.downloadingCount &&
    a.completedCount === b.completedCount
  )
}

export function useStatusBarPoller(
  refreshIntervalSeconds: number,
  enabled: boolean,
  fetchCounts = true,
) {
  const [status, setStatus] = useState<DaemonStatusDto>(disconnected)
  const mountedRef = useRef(true)
  const inFlightRef = useRef(false)
  const intervalMs = Math.max(refreshIntervalSeconds, 1) * 1000

  const poll = useCallback(async () => {
    if (!enabled || inFlightRef.current) return

    inFlightRef.current = true
    try {
      const daemonStatus = await api.getStatus({ counts: fetchCounts })
      if (!mountedRef.current) return

      startTransition(() => {
        setStatus((prev) => (statusEquals(prev, daemonStatus) ? prev : daemonStatus))
      })
    } catch {
      if (!mountedRef.current) return
      startTransition(() => {
        setStatus((prev) => (statusEquals(prev, disconnected) ? prev : disconnected))
      })
    } finally {
      inFlightRef.current = false
    }
  }, [enabled, fetchCounts])

  useEffect(() => {
    if (!enabled) return

    mountedRef.current = true
    void poll()
    const timer = window.setInterval(() => void poll(), intervalMs)
    return () => {
      mountedRef.current = false
      window.clearInterval(timer)
    }
  }, [enabled, poll, intervalMs])

  return { status }
}
