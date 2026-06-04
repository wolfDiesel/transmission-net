import { Button, Dialog, Field, Flex } from '@chakra-ui/react'
import { OverlayPortal } from '../ui/OverlayPortal'
import { useEffect, useState } from 'react'
import { useI18n } from '../../i18n'
import type { TorrentDto } from '../../api/types'
import { DownloadDirCombobox } from '../AddTorrent/DownloadDirCombobox'

type TorrentMoveDialogProps = {
  torrent: TorrentDto | null
  open: boolean
  busy?: boolean
  directories: readonly string[]
  onClose: () => void
  onConfirm: (location: string, move: boolean) => void
}

export function TorrentMoveDialog({
  torrent,
  open,
  busy,
  directories,
  onClose,
  onConfirm,
}: TorrentMoveDialogProps) {
  const { t } = useI18n()
  const [location, setLocation] = useState('')
  const [moveData, setMoveData] = useState(true)

  useEffect(() => {
    if (torrent && open) {
      setLocation(torrent.downloadDir)
      setMoveData(true)
    }
  }, [torrent, open])

  if (!torrent) return null

  return (
    <Dialog.Root open={open} onOpenChange={(e) => !e.open && onClose()}>
      <OverlayPortal>
        <Dialog.Backdrop />
        <Dialog.Positioner>
          <Dialog.Content bg="bg.emphasized" borderColor="border">
            <Dialog.Header>
              <Dialog.Title color="fg">{t('torrentTable.moveDialog.title')}</Dialog.Title>
            </Dialog.Header>
            <Dialog.Body>
              <Field.Root>
                <Field.Label>{t('torrentTable.moveDialog.destination')}</Field.Label>
                <DownloadDirCombobox
                  value={location}
                  onChange={setLocation}
                  directories={directories}
                />
              </Field.Root>
              <Field.Root mt={3}>
                <Field.Label display="flex" alignItems="center" gap={2} cursor="pointer">
                  <input
                    type="checkbox"
                    checked={moveData}
                    onChange={(e) => setMoveData(e.target.checked)}
                  />
                  {t('torrentTable.moveDialog.moveData')}
                </Field.Label>
              </Field.Root>
            </Dialog.Body>
            <Dialog.Footer>
              <Flex gap={2}>
                <Button variant="outline" borderColor="border" onClick={onClose}>
                  {t('common.cancel')}
                </Button>
                <Button
                  colorPalette="brand"
                  loading={busy}
                  disabled={!location.trim()}
                  onClick={() => onConfirm(location.trim(), moveData)}
                >
                  {t('torrentTable.moveDialog.confirm')}
                </Button>
              </Flex>
            </Dialog.Footer>
          </Dialog.Content>
        </Dialog.Positioner>
      </OverlayPortal>
    </Dialog.Root>
  )
}
