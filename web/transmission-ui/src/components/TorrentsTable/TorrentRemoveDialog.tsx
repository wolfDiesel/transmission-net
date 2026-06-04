import {
  Box,
  Button,
  Checkbox,
  Dialog,
  Flex,
  Text,
} from '@chakra-ui/react'
import { OverlayPortal } from '../ui/OverlayPortal'
import { useEffect, useState } from 'react'
import { useI18n } from '../../i18n'
import type { TorrentDto } from '../../api/types'

type TorrentRemoveDialogProps = {
  torrent: TorrentDto | null
  open: boolean
  busy?: boolean
  onClose: () => void
  onConfirm: (deleteLocalData: boolean) => void
}

export function TorrentRemoveDialog({
  torrent,
  open,
  busy,
  onClose,
  onConfirm,
}: TorrentRemoveDialogProps) {
  const { t } = useI18n()
  const [deleteLocalData, setDeleteLocalData] = useState(false)

  useEffect(() => {
    if (open) setDeleteLocalData(false)
  }, [open, torrent?.id])

  if (!torrent) return null

  return (
    <Dialog.Root open={open} onOpenChange={(e) => !e.open && !busy && onClose()}>
      <OverlayPortal>
        <Dialog.Backdrop />
        <Dialog.Positioner>
          <Dialog.Content
            bg="bg.emphasized"
            borderColor="border"
            borderRadius="md"
            maxW="440px"
          >
            <Dialog.Header borderBottomWidth="1px" borderColor="border" pb={3}>
              <Dialog.Title color="brand.500">{t('torrentTable.removeDialog.title')}</Dialog.Title>
            </Dialog.Header>
            <Dialog.Body py={4}>
              <Text fontSize="sm" color="fg.muted" mb={2}>
                {t('torrentTable.removeDialog.aboutToRemove')}
              </Text>
              <Box
                px={3}
                py={2}
                borderRadius="md"
                borderWidth="1px"
                borderColor="border"
                bg="surface.panel"
                mb={4}
              >
                <Text fontSize="sm" color="fg" fontWeight="medium" lineClamp={3}>
                  {torrent.name}
                </Text>
              </Box>
              <Checkbox.Root
                checked={deleteLocalData}
                onCheckedChange={(e) => setDeleteLocalData(Boolean(e.checked))}
              >
                <Checkbox.HiddenInput />
                <Checkbox.Control borderColor="border" />
                <Checkbox.Label color="fg" fontSize="sm">
                  {t('torrentTable.removeDialog.deleteData')}
                </Checkbox.Label>
              </Checkbox.Root>
              {deleteLocalData && (
                <Text fontSize="xs" color="red.400" mt={2}>
                  {t('torrentTable.removeDialog.deleteWarning')}
                </Text>
              )}
            </Dialog.Body>
            <Dialog.Footer borderTopWidth="1px" borderColor="border" pt={3}>
              <Flex gap={2} w="full" justify="flex-end">
                <Button
                  variant="outline"
                  borderColor="border"
                  onClick={onClose}
                  disabled={busy}
                >
                  {t('common.cancel')}
                </Button>
                <Button
                  colorPalette="red"
                  loading={busy}
                  onClick={() => onConfirm(deleteLocalData)}
                >
                  {t('torrentTable.removeDialog.confirm')}
                </Button>
              </Flex>
            </Dialog.Footer>
          </Dialog.Content>
        </Dialog.Positioner>
      </OverlayPortal>
    </Dialog.Root>
  )
}
