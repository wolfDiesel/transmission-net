import { useEffect, useRef } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../../api/client'

export function TorrentLaunchRedirect() {
  const navigate = useNavigate()
  const handled = useRef(false)

  useEffect(() => {
    if (handled.current) return
    handled.current = true

    void api.getPendingTorrentPath().then(({ path }) => {
      if (path) {
        navigate(`/add?torrentPath=${encodeURIComponent(path)}`, { replace: true })
      }
    })
  }, [navigate])

  return null
}
