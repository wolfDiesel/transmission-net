import { useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../../api/client'

const POLL_MS = 1500

export function TorrentLaunchWatcher() {
  const navigate = useNavigate()

  useEffect(() => {
    const handlePending = (path: string | null) => {
      if (!path) return
      navigate(`/add?torrentPath=${encodeURIComponent(path)}`, { replace: false })
    }

    const id = window.setInterval(() => {
      void api.getPendingTorrentPath({ consume: true }).then(({ path }) => handlePending(path))
    }, POLL_MS)

    return () => window.clearInterval(id)
  }, [navigate])

  return null
}
