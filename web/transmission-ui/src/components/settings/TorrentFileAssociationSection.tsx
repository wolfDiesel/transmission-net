import { Box, Button, Text } from '@chakra-ui/react'
import { useCallback, useEffect, useState } from 'react'
import { api, ApiError } from '../../api/client'
import type { TorrentFileAssociationStatusDto } from '../../api/types'
import { useI18n } from '../../i18n'
import { showAppToast } from '../AppToast'

type TorrentFileAssociationSectionProps = {
  onRegistered: () => void
}

export function TorrentFileAssociationSection({ onRegistered }: TorrentFileAssociationSectionProps) {
  const { t } = useI18n()
  const [status, setStatus] = useState<TorrentFileAssociationStatusDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [registering, setRegistering] = useState(false)

  const loadStatus = useCallback(async () => {
    setLoading(true)
    try {
      const loaded = await api.getTorrentFileAssociationStatus()
      setStatus(loaded)
    } catch {
      setStatus(null)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadStatus()
  }, [loadStatus])

  const handleRegister = async () => {
    setRegistering(true)
    try {
      await api.registerTorrentFileAssociation()
      onRegistered()
      const refreshed = await api.getTorrentFileAssociationStatus()
      setStatus(refreshed)
      if (refreshed.isDefaultHandler) {
        showAppToast({
          title: t('settings.torrentAssociation.success'),
          variant: 'success',
        })
      } else {
        showAppToast({
          title: t('settings.torrentAssociation.partial'),
          variant: 'error',
        })
      }
    } catch (e) {
      showAppToast({
        title: e instanceof ApiError ? e.message : t('settings.torrentAssociation.failed'),
        variant: 'error',
      })
    } finally {
      setRegistering(false)
    }
  }

  if (loading) {
    return (
      <Box
        borderWidth="1px"
        borderColor="border"
        borderRadius="md"
        bg="bg.emphasized"
        px={5}
        py={5}
      >
        <Text fontSize="sm" color="fg.muted">
          {t('settings.torrentAssociation.checking')}
        </Text>
      </Box>
    )
  }

  if (!status?.isSupported) {
    return null
  }

  const statusLine = status.isDefaultHandler
    ? t('settings.torrentAssociation.defaultHandler')
    : status.hasDesktopEntry
      ? t('settings.torrentAssociation.hasEntryNotDefault')
      : t('settings.torrentAssociation.noEntry')

  return (
    <Box
      borderWidth="1px"
      borderColor="border"
      borderRadius="md"
      bg="bg.emphasized"
      px={5}
      py={5}
    >
      <Text fontSize="sm" fontWeight="semibold" color="brand.500" mb={2}>
        {t('settings.torrentAssociation.title')}
      </Text>
      <Text fontSize="sm" color="fg.muted" mb={4}>
        {statusLine} {t('settings.torrentAssociation.hint')}
      </Text>
      <Button
        colorPalette="brand"
        size="sm"
        loading={registering}
        onClick={() => void handleRegister()}
      >
        {status.hasDesktopEntry
          ? t('settings.torrentAssociation.refreshRegister')
          : t('settings.torrentAssociation.register')}
      </Button>
    </Box>
  )
}
