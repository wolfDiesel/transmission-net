import { Button, Dialog, Text } from '@chakra-ui/react'
import { useEffect, useState } from 'react'
import { api, ApiError } from '../../api/client'
import { showAppToast } from '../AppToast'
import { OverlayPortal } from '../ui/OverlayPortal'

export function TorrentAssociationPrompt() {
  const [open, setOpen] = useState(false)
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    let cancelled = false
    void api
      .getTorrentFileAssociationStatus()
      .then((status) => {
        if (!cancelled && status.shouldPrompt) setOpen(true)
      })
      .catch(() => {})
    return () => {
      cancelled = true
    }
  }, [])

  const handleAccept = async () => {
    setBusy(true)
    try {
      await api.registerTorrentFileAssociation()
      setOpen(false)
      showAppToast({
        title: 'Файлы .torrent будут открываться в TransmissionNET',
        variant: 'success',
      })
    } catch (e) {
      showAppToast({
        title: e instanceof ApiError ? e.message : 'Не удалось зарегистрировать ассоциацию',
        variant: 'error',
      })
    } finally {
      setBusy(false)
    }
  }

  const handleDecline = async () => {
    setBusy(true)
    try {
      await api.declineTorrentFileAssociation()
      setOpen(false)
    } catch (e) {
      showAppToast({
        title: e instanceof ApiError ? e.message : 'Не удалось сохранить настройку',
        variant: 'error',
      })
    } finally {
      setBusy(false)
    }
  }

  return (
    <Dialog.Root
      open={open}
      onOpenChange={(e) => {
        if (!e.open && !busy) void handleDecline()
      }}
    >
      <OverlayPortal>
        <Dialog.Backdrop />
        <Dialog.Positioner>
          <Dialog.Content bg="bg.emphasized" borderColor="border">
            <Dialog.Header>
              <Dialog.Title color="fg">Открывать торренты в TransmissionNET?</Dialog.Title>
            </Dialog.Header>
            <Dialog.Body>
              <Text fontSize="sm" color="fg.muted">
                Сделать TransmissionNET программой по умолчанию для файлов .torrent? Выбор сохранится в
                настройках, повторно спрашивать не будем.
              </Text>
            </Dialog.Body>
            <Dialog.Footer>
              <Button
                variant="outline"
                borderColor="border"
                disabled={busy}
                onClick={() => void handleDecline()}
              >
                Нет
              </Button>
              <Button colorPalette="brand" loading={busy} onClick={() => void handleAccept()}>
                Да
              </Button>
            </Dialog.Footer>
          </Dialog.Content>
        </Dialog.Positioner>
      </OverlayPortal>
    </Dialog.Root>
  )
}
