import { useCallback, useEffect, useRef, useState } from 'react'
import { api, ApiError } from '../api/client'
import type { TorrentDetailsDto } from '../api/types'

export function useTorrentDetails(torrentId: number | null, open: boolean) {
  const [details, setDetails] = useState<TorrentDetailsDto | null>(null)
  const [loading, setLoading] = useState(false)
  const [refreshing, setRefreshing] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [reloadToken, setReloadToken] = useState(0)
  const detailsRef = useRef(details)
  detailsRef.current = details

  const refresh = useCallback(() => {
    setReloadToken((n) => n + 1)
  }, [])

  useEffect(() => {
    if (torrentId === null) {
      setDetails(null)
      setError(null)
      setLoading(false)
      setRefreshing(false)
      return
    }

    if (!open) {
      return
    }

    let cancelled = false
    const showFullLoader = detailsRef.current === null
    if (showFullLoader) setLoading(true)
    else setRefreshing(true)
    setError(null)

    void api
      .getTorrentDetails(torrentId)
      .then((loaded) => {
        if (!cancelled) setDetails(loaded)
      })
      .catch((e) => {
        if (!cancelled) {
          if (showFullLoader) setDetails(null)
          setError(e instanceof ApiError ? e.message : 'Failed to load torrent details')
        }
      })
      .finally(() => {
        if (!cancelled) {
          setLoading(false)
          setRefreshing(false)
        }
      })

    return () => {
      cancelled = true
    }
  }, [torrentId, open, reloadToken])

  return { details, loading, refreshing, error, refresh }
}
