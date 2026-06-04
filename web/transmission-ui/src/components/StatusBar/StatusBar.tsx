import { Box, Flex, Text } from '@chakra-ui/react'
import { useMemo } from 'react'
import { useLocation } from 'react-router-dom'
import { useApp } from '../../context/AppProvider'
import { useI18n } from '../../i18n'
import { useTorrentListOptional } from '../../context/TorrentListProvider'
import { deriveTorrentCounts } from '../../features/torrentStatus/deriveTorrentCounts'
import { deriveTorrentSpeeds } from '../../features/torrentStatus/deriveTorrentSpeeds'
import { islandPanelStyle } from '../layout/islandStyles'
import { formatBytesPerSec } from '../../utils/format'
import { ConnectionSemaphore } from './ConnectionSemaphore'
import { useStatusBarPoller } from './useStatusBarPoller'

export function StatusBar() {
  const { t } = useI18n()
  const { refreshIntervalSeconds } = useApp()
  const location = useLocation()
  const torrentList = useTorrentListOptional()
  const onTorrentsPage = location.pathname === '/'
  const fetchStatusCounts = !onTorrentsPage
  const { status } = useStatusBarPoller(
    refreshIntervalSeconds,
    !onTorrentsPage,
    fetchStatusCounts,
  )

  const display = useMemo(() => {
    if (onTorrentsPage && torrentList && !torrentList.loading) {
      const counts = deriveTorrentCounts(torrentList.torrents)
      const speeds = deriveTorrentSpeeds(torrentList.torrents)
      return {
        connected: torrentList.daemonConnected,
        downloadSpeed: speeds.downloadSpeed,
        uploadSpeed: speeds.uploadSpeed,
        downloading: counts.downloading,
        completed: counts.completed,
      }
    }

    return {
      connected: status.connected,
      downloadSpeed: status.downloadSpeed,
      uploadSpeed: status.uploadSpeed,
      downloading: status.downloadingCount,
      completed: status.completedCount,
    }
  }, [onTorrentsPage, torrentList, status])

  return (
    <Box as="footer" flexShrink={0} {...islandPanelStyle}>
      <Flex
        align="center"
        justify="space-between"
        gap={4}
        px={4}
        py={2}
        fontSize="sm"
        flexWrap="wrap"
      >
        <ConnectionSemaphore connected={display.connected} />

        <Flex align="center" gap={6} color="fg.muted" flexWrap="wrap">
          <Text>↓ {formatBytesPerSec(display.downloadSpeed)}</Text>
          <Text>↑ {formatBytesPerSec(display.uploadSpeed)}</Text>
          <Text>
            {t('statusBar.downloading')}: <Text as="span" color="brand.500">{display.downloading}</Text>
          </Text>
          <Text>
            {t('statusBar.completed')}: <Text as="span" color="brand.500">{display.completed}</Text>
          </Text>
        </Flex>
      </Flex>
    </Box>
  )
}
