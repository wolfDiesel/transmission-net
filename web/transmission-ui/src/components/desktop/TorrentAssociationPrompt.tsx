import { Button, Dialog, Text } from '@chakra-ui/react'
import { useEffect, useState } from 'react'
import { api, ApiError } from '../../api/client'
import { useI18n } from '../../i18n'
import { showAppToast } from '../AppToast'
import { OverlayPortal } from '../ui/OverlayPortal'

export function TorrentAssociationPrompt() {
  const { t } = useI18n()
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
      const status = await api.getTorrentFileAssociationStatus()
      setOpen(false)
      showAppToast({
        title: status.isDefaultHandler
          ? t('settings.torrentAssociation.success')
          : t('settings.torrentAssociation.partial'),
        variant: status.isDefaultHandler ? 'success' : 'error',
      })
    } catch (e) {
      showAppToast({
        title: e instanceof ApiError ? e.message : t('settings.torrentAssociation.failed'),
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
        title: e instanceof ApiError ? e.message : t('torrentAssociationPrompt.saveFailed'),
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
              <Dialog.Title color="fg">{t('torrentAssociationPrompt.title')}</Dialog.Title>
            </Dialog.Header>
            <Dialog.Body>
              <Text fontSize="sm" color="fg.muted">
                {t('torrentAssociationPrompt.body')}
              </Text>
            </Dialog.Body>
            <Dialog.Footer>
              <Button
                variant="outline"
                borderColor="border"
                disabled={busy}
                onClick={() => void handleDecline()}
              >
                {t('common.no')}
              </Button>
              <Button colorPalette="brand" loading={busy} onClick={() => void handleAccept()}>
                {t('common.yes')}
              </Button>
            </Dialog.Footer>
          </Dialog.Content>
        </Dialog.Positioner>
      </OverlayPortal>
    </Dialog.Root>
  )
}
