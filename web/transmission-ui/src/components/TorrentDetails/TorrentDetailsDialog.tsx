import {
  Button,
  Dialog,
  Flex,
  Spinner,
  Tabs,
  Text,
} from '@chakra-ui/react'
import { useCallback, useEffect, useRef, useState } from 'react'
import { api, ApiError } from '../../api/client'
import { executeTorrentAction } from '../../api/torrentActions'
import type { TorrentDto, TorrentFileNodeDto } from '../../api/types'
import { useI18n } from '../../i18n'
import { showAppToast } from '../AppToast'
import { useTorrentDetails } from '../../hooks/useTorrentDetails'
import { TorrentDetailsGeneralTab } from './TorrentDetailsGeneralTab'
import { TorrentDetailsTransferTab } from './TorrentDetailsTransferTab'
import { TorrentFileRenamePanel } from './TorrentFileRenamePanel'
import { TorrentFileTree } from './TorrentFileTree'
import { TorrentMassRenamePanel } from './TorrentMassRenamePanel'
import { OverlayPortal } from '../ui/OverlayPortal'

type TorrentDetailsDialogProps = {
  torrent: TorrentDto | null
  open: boolean
  onClose: () => void
  onExited?: () => void
  onTorrentsChanged?: () => void
}

export function TorrentDetailsDialog({
  torrent,
  open,
  onClose,
  onExited,
  onTorrentsChanged,
}: TorrentDetailsDialogProps) {
  const { t } = useI18n()
  const dialogContentRef = useRef<HTMLDivElement>(null)
  const { details, loading, refreshing, error, refresh } = useTorrentDetails(
    torrent?.id ?? null,
    open,
  )
  const [renameNode, setRenameNode] = useState<TorrentFileNodeDto | null>(null)
  const [massRenameScope, setMassRenameScope] = useState<string | null>(null)
  const [renameBusy, setRenameBusy] = useState(false)
  const [massRenameBusy, setMassRenameBusy] = useState(false)
  const title = details?.name ?? torrent?.name ?? 'Torrent'

  const overlayOpen = renameNode !== null || massRenameScope !== null
  const anyBusy = renameBusy || massRenameBusy

  useEffect(() => {
    if (!open) {
      setRenameNode(null)
      setMassRenameScope(null)
    }
  }, [open])

  const handleRenameConfirm = useCallback(
    async (path: string, name: string) => {
      if (!torrent) return
      setRenameBusy(true)
      try {
        await executeTorrentAction({
          action: 'rename-path',
          ids: [torrent.id],
          path,
          name,
        })
        showAppToast({ title: t('torrentDetails.renamed'), variant: 'success' })
        setRenameNode(null)
        refresh()
        onTorrentsChanged?.()
      } catch (e) {
        showAppToast({
          title: e instanceof ApiError ? e.message : t('torrentDetails.renameFailed'),
          variant: 'error',
        })
      } finally {
        setRenameBusy(false)
      }
    },
    [torrent, refresh, onTorrentsChanged, t],
  )

  const handleMassRenameApply = useCallback(
    async (operations: { path: string; name: string }[]) => {
      if (!torrent) return
      setMassRenameBusy(true)
      try {
        const result = await api.renameTorrentBatch(torrent.id, operations)
        if (result.failures.length === 0) {
          showAppToast({
            title: t('torrentDetails.renamedCount', { count: result.applied }),
            variant: 'success',
          })
        } else {
          showAppToast({
            title: t('torrentDetails.renamedPartial', {
              applied: result.applied,
              failed: result.failures.length,
            }),
            variant: 'error',
          })
        }
        setMassRenameScope(null)
        refresh()
        onTorrentsChanged?.()
      } catch (e) {
        showAppToast({
          title: e instanceof ApiError ? e.message : t('torrentDetails.massRenameFailed'),
          variant: 'error',
        })
      } finally {
        setMassRenameBusy(false)
      }
    },
    [torrent, refresh, onTorrentsChanged, t],
  )

  return (
    <Dialog.Root
      open={open}
      onOpenChange={(e) => {
        if (!e.open && !overlayOpen && !anyBusy) onClose()
      }}
      onExitComplete={onExited}
      size="xl"
    >
      <OverlayPortal>
        <Dialog.Backdrop />
        <Dialog.Positioner p={4}>
          <Dialog.Content
            ref={dialogContentRef}
            bg="surface.panel"
            borderColor="border"
            borderRadius="lg"
            maxW="760px"
            w="full"
            maxH="85vh"
            display="flex"
            flexDirection="column"
            position="relative"
            overflow="hidden"
          >
            <Dialog.Header
              borderBottomWidth="1px"
              borderColor="border"
              pb={3}
              flexShrink={0}
            >
              <Flex align="center" justify="space-between" gap={3}>
                <Dialog.Title color="fg" fontSize="md" lineClamp={2} flex="1">
                  {title}
                </Dialog.Title>
                {refreshing && (
                  <Spinner size="sm" color="brand.500" flexShrink={0} />
                )}
              </Flex>
            </Dialog.Header>

            <Dialog.Body flex="1" minH={0} py={0} display="flex" flexDirection="column">
              {loading && !details && (
                <Flex py={12} justify="center">
                  <Spinner color="brand.500" />
                </Flex>
              )}

              {error && !details && (
                <Text py={8} textAlign="center" color="fg.muted" fontSize="sm">
                  {error}
                </Text>
              )}

              {details && (
                <Tabs.Root
                  defaultValue="general"
                  variant="line"
                  colorPalette="brand"
                  size="sm"
                  flex="1"
                  minH={0}
                  display="flex"
                  flexDirection="column"
                  opacity={refreshing ? 0.92 : 1}
                >
                  <Tabs.List px={4} pt={3} flexShrink={0}>
                    <Tabs.Trigger value="general">{t('torrentDetails.tabs.general')}</Tabs.Trigger>
                    <Tabs.Trigger value="transfer">{t('torrentDetails.tabs.transfer')}</Tabs.Trigger>
                    <Tabs.Trigger value="files">{t('torrentDetails.tabs.files')}</Tabs.Trigger>
                  </Tabs.List>

                  <Tabs.Content value="general" flex="1" minH={0} overflow="auto" px={4} py={3}>
                    <TorrentDetailsGeneralTab details={details} />
                  </Tabs.Content>

                  <Tabs.Content value="transfer" flex="1" minH={0} overflow="auto" px={4} py={3}>
                    <TorrentDetailsTransferTab details={details} />
                  </Tabs.Content>

                  <Tabs.Content
                    value="files"
                    flex="1"
                    minH={0}
                    overflow="auto"
                    px={2}
                    py={2}
                  >
                    <TorrentFileTree
                      nodes={details.fileTree}
                      torrentName={details.name}
                      menuPortalContainer={dialogContentRef}
                      onRename={(node) => setRenameNode(node)}
                      onMassRename={(scopePath) => setMassRenameScope(scopePath)}
                    />
                  </Tabs.Content>
                </Tabs.Root>
              )}
            </Dialog.Body>

            <Dialog.Footer borderTopWidth="1px" borderColor="border" pt={3} flexShrink={0}>
              <Flex justify="flex-end" w="full">
                <Button variant="outline" borderColor="border" onClick={onClose}>
                  Close
                </Button>
              </Flex>
            </Dialog.Footer>

          </Dialog.Content>
        </Dialog.Positioner>

        {renameNode && (
          <TorrentFileRenamePanel
            node={renameNode}
            busy={renameBusy}
            onClose={() => setRenameNode(null)}
            onConfirm={(path, name) => void handleRenameConfirm(path, name)}
          />
        )}

        {massRenameScope !== null && details && (
          <TorrentMassRenamePanel
            scopePath={massRenameScope}
            fileTree={details.fileTree}
            busy={massRenameBusy}
            onClose={() => setMassRenameScope(null)}
            onApply={(operations) => void handleMassRenameApply(operations)}
          />
        )}
      </OverlayPortal>
    </Dialog.Root>
  )
}
