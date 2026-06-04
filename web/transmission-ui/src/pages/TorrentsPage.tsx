import { Box, Flex, Spinner, Text } from '@chakra-ui/react'
import { ColumnSettingsPanel, TorrentsTable } from '../components/TorrentsTable/TorrentsTable'
import { useTorrentList } from '../context/TorrentListProvider'
import { useTorrentTableSettings } from '../hooks/useTorrentTableSettings'
import { useApp } from '../context/AppProvider'
import { useI18n } from '../i18n'
import { formatLastUpdated } from '../utils/format'

export function TorrentsPage() {
  const { t } = useI18n()
  const { refreshIntervalSeconds } = useApp()
  const { torrents, loading, refreshing, lastUpdated, refreshNow } = useTorrentList()
  const { tableSettings, setColumns } = useTorrentTableSettings()

  if (loading) {
    return (
      <Flex justify="center" align="center" flex="1">
        <Spinner color="brand.500" />
      </Flex>
    )
  }

  return (
    <Box display="flex" flexDirection="column" flex="1" minH={0} gap={3}>
      <Flex justify="space-between" align="center" flexShrink={0} gap={3} flexWrap="wrap">
        <Box>
          <Text fontSize="lg" fontWeight="semibold" color="fg">
            {t('torrentsPage.title')}
          </Text>
          <Text fontSize="xs" color="fg.muted">
            {t('torrentsPage.updated', {
              time: formatLastUpdated(lastUpdated),
              seconds: refreshIntervalSeconds,
            })}
          </Text>
        </Box>
        <ColumnSettingsPanel tableSettings={tableSettings} onChange={setColumns} />
      </Flex>

      {torrents.length === 0 ? (
        <Flex flex="1" align="center" justify="center" minH={0}>
          <Text color="fg.muted" fontSize="sm">
            {t('torrentsPage.empty')}
          </Text>
        </Flex>
      ) : (
        <TorrentsTable
          torrents={torrents}
          refreshing={refreshing}
          onTorrentsChanged={() => void refreshNow()}
        />
      )}
    </Box>
  )
}
