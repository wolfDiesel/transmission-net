import { Box, Button, Text } from '@chakra-ui/react'
import { useCallback, useEffect, useState } from 'react'
import { api, ApiError } from '../../api/client'
import type { TorrentFileAssociationStatusDto } from '../../api/types'
import { showAppToast } from '../AppToast'

type TorrentFileAssociationSectionProps = {
  onRegistered: () => void
}

export function TorrentFileAssociationSection({ onRegistered }: TorrentFileAssociationSectionProps) {
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
      await loadStatus()
      showAppToast({
        title: 'Файлы .torrent зарегистрированы для TransmissionNET',
        variant: 'success',
      })
    } catch (e) {
      showAppToast({
        title: e instanceof ApiError ? e.message : 'Не удалось зарегистрировать ассоциацию',
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
          Проверка интеграции с рабочим столом…
        </Text>
      </Box>
    )
  }

  if (!status?.isSupported) {
    return null
  }

  const statusLine = status.isDefaultHandler
    ? 'TransmissionNET — обработчик .torrent по умолчанию.'
    : status.hasDesktopEntry
      ? 'Ярлык есть, но приложение не выбрано по умолчанию для .torrent.'
      : 'Ярлык не найден — будет создан при регистрации.'

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
        Файлы .torrent
      </Text>
      <Text fontSize="sm" color="fg.muted" mb={4}>
        {statusLine} Перед регистрацией проверяются все ярлыки в каталоге приложений и
        обновляются те, что относятся к TransmissionNET.
      </Text>
      <Button
        colorPalette="brand"
        size="sm"
        loading={registering}
        onClick={() => void handleRegister()}
      >
        {status.hasDesktopEntry ? 'Обновить и зарегистрировать' : 'Зарегистрировать ассоциацию'}
      </Button>
    </Box>
  )
}
