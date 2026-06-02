import { Box, Table, Text } from '@chakra-ui/react'
import {
  startTransition,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
  type MouseEvent,
} from 'react'
import { useTorrentList } from '../../context/TorrentListProvider'
import { ApiError, executeTorrentAction } from '../../api/torrentActions'
import type { TorrentDto, TorrentBandwidthPriority } from '../../api/types'
import { showAppToast } from '../AppToast'
import { buildColumnWidthMap } from '../../features/torrentTable/columnWidths'
import { getColumnDef } from '../../features/torrentTable/columns'
import { sortTorrents } from '../../features/torrentTable/sortTorrents'
import { useDownloadDirHistory } from '../../hooks/useDownloadDirHistory'
import { useTorrentTableSettings } from '../../hooks/useTorrentTableSettings'
import { ResizableColumnHeader } from './ResizableColumnHeader'
import { ScrollToTopButton } from './ScrollToTopButton'
import { TorrentMoveDialog } from './TorrentMoveDialog'
import { TorrentDetailsDialog } from '../TorrentDetails'
import { TorrentRemoveDialog } from './TorrentRemoveDialog'
import { FloatingContextMenu } from '../ui/FloatingContextMenu'
import { toFloatingContextMenuItems } from '../ui/ContextMenu/toFloatingContextMenuItems'
import { buildTorrentContextMenuItems } from './buildTorrentContextMenuItems'
import { TorrentTableRow } from './TorrentTableRow'
import { useColumnResize } from './useColumnResize'

type TorrentsTableProps = {
  torrents: TorrentDto[]
  refreshing?: boolean
  onTorrentsChanged?: () => void
}

const SCROLL_TOP_THRESHOLD = 80

type RowContextTarget = {
  torrent: TorrentDto
  x: number
  y: number
}

export function TorrentsTable({ torrents, onTorrentsChanged }: TorrentsTableProps) {
  const { setTorrentPollingPaused } = useTorrentList()
  const { tableSettings, visibleColumnIds, setSort, setColumnWidth } = useTorrentTableSettings()
  const { directories, remember } = useDownloadDirHistory()
  const scrollRef = useRef<HTMLDivElement>(null)
  const [showScrollTop, setShowScrollTop] = useState(false)
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const [detailsTorrent, setDetailsTorrent] = useState<TorrentDto | null>(null)
  const [detailsOpen, setDetailsOpen] = useState(false)
  const closingTorrentIdRef = useRef<number | null>(null)
  const [moveTorrent, setMoveTorrent] = useState<TorrentDto | null>(null)
  const [removeTorrent, setRemoveTorrent] = useState<TorrentDto | null>(null)
  const [contextTarget, setContextTarget] = useState<RowContextTarget | null>(null)
  const menuRowId = contextTarget?.torrent.id ?? null
  const [actionBusy, setActionBusy] = useState(false)

  useEffect(() => {
    const paused = detailsTorrent !== null
    setTorrentPollingPaused(paused)
    return () => setTorrentPollingPaused(false)
  }, [detailsTorrent, setTorrentPollingPaused])

  const openDetails = useCallback((torrent: TorrentDto) => {
    closingTorrentIdRef.current = null
    setDetailsTorrent(torrent)
    setDetailsOpen(true)
  }, [])

  const closeDetails = useCallback(() => {
    closingTorrentIdRef.current = detailsTorrent?.id ?? null
    setDetailsOpen(false)
  }, [detailsTorrent?.id])

  const handleDetailsExited = useCallback(() => {
    const closingId = closingTorrentIdRef.current
    closingTorrentIdRef.current = null
    startTransition(() => {
      setDetailsTorrent((current) => (current?.id === closingId ? null : current))
    })
  }, [])

  const { previewWidths, startResize } = useColumnResize(setColumnWidth)

  const handleScroll = useCallback(() => {
    const el = scrollRef.current
    if (!el) return
    setShowScrollTop(el.scrollTop > SCROLL_TOP_THRESHOLD)
  }, [])

  const scrollToTop = useCallback(() => {
    scrollRef.current?.scrollTo({ top: 0, behavior: 'smooth' })
  }, [])

  const sortedTorrents = useMemo(
    () => sortTorrents(torrents, tableSettings.sortColumnId, tableSettings.sortDescending),
    [torrents, tableSettings.sortColumnId, tableSettings.sortDescending],
  )

  const columnWidths = useMemo(
    () => buildColumnWidthMap(tableSettings, visibleColumnIds, previewWidths ?? undefined),
    [tableSettings, visibleColumnIds, previewWidths],
  )

  const headers = visibleColumnIds
    .map((id) => getColumnDef(id))
    .filter((def): def is NonNullable<typeof def> => def !== undefined)

  const runAction = useCallback(
    async (fn: () => Promise<void>, successMessage: string) => {
      setActionBusy(true)
      try {
        await fn()
        showAppToast({ title: successMessage, variant: 'success' })
        onTorrentsChanged?.()
      } catch (e) {
        showAppToast({
          title: e instanceof ApiError ? e.message : 'Action failed',
          variant: 'error',
        })
      } finally {
        setActionBusy(false)
      }
    },
    [onTorrentsChanged],
  )

  const actOn = useCallback(
    (torrent: TorrentDto, action: Parameters<typeof executeTorrentAction>[0]) =>
      runAction(() => executeTorrentAction({ ...action, ids: [torrent.id] }), 'Done'),
    [runAction],
  )

  const handleRowSelect = useCallback(
    (torrent: TorrentDto) => {
      setSelectedId(torrent.id)
      openDetails(torrent)
    },
    [openDetails],
  )

  const closeContextMenu = useCallback(() => setContextTarget(null), [])

  const handleRowContextMenu = useCallback((torrent: TorrentDto, event: MouseEvent) => {
    event.preventDefault()
    event.stopPropagation()
    setContextTarget({ torrent, x: event.clientX, y: event.clientY })
  }, [])

  const contextMenuItems = useMemo(() => {
    if (!contextTarget) return []
    const torrent = contextTarget.torrent
    return toFloatingContextMenuItems(
      buildTorrentContextMenuItems(torrent, {
        onStart: () => void actOn(torrent, { action: 'start', ids: [torrent.id] }),
        onStop: () => void actOn(torrent, { action: 'stop', ids: [torrent.id] }),
        onVerify: () => void actOn(torrent, { action: 'verify', ids: [torrent.id] }),
        onMove: () => {
          closeContextMenu()
          setMoveTorrent(torrent)
        },
        onRemove: () => {
          closeContextMenu()
          setRemoveTorrent(torrent)
        },
        onSetPriority: (priority: TorrentBandwidthPriority) =>
          void actOn(torrent, { action: 'set-priority', ids: [torrent.id], priority }),
      }),
    )
  }, [actOn, closeContextMenu, contextTarget])

  const handleRemoveConfirm = useCallback(
    (deleteLocalData: boolean) => {
      if (!removeTorrent) return
      void runAction(async () => {
        await executeTorrentAction({
          action: 'remove',
          ids: [removeTorrent.id],
          deleteLocalData,
        })
        setRemoveTorrent(null)
        if (selectedId === removeTorrent.id) setSelectedId(null)
        if (detailsTorrent?.id === removeTorrent.id) setDetailsTorrent(null)
      }, 'Torrent removed')
    },
    [detailsTorrent?.id, removeTorrent, runAction, selectedId],
  )

  const handleMoveConfirm = useCallback(
    (location: string, move: boolean) => {
      if (!moveTorrent) return
      void runAction(async () => {
        await executeTorrentAction({
          action: 'move',
          ids: [moveTorrent.id],
          location,
          move,
        })
        remember(location)
        setMoveTorrent(null)
      }, 'Torrent moved')
    },
    [moveTorrent, remember, runAction],
  )

  const suppressTableScrollbar =
    contextTarget !== null ||
    detailsOpen ||
    moveTorrent !== null ||
    removeTorrent !== null

  return (
    <Box
      flex="1"
      minH={0}
      display="flex"
      flexDirection="column"
      position="relative"
      zIndex={0}
      isolation="isolate"
      opacity={actionBusy ? 0.85 : 1}
      transition={actionBusy ? 'opacity 0.15s' : undefined}
    >
      <Table.ScrollArea
        ref={scrollRef}
        flex="1"
        minH={0}
        onScroll={handleScroll}
        css={{
          position: 'relative',
          zIndex: 0,
          overflowX: suppressTableScrollbar ? 'hidden' : 'auto',
          overflowY: 'auto',
        }}
      >
        <Table.Root
          size="sm"
          variant="line"
          stickyHeader
          css={{ tableLayout: 'fixed', width: 'max-content', minWidth: '100%' }}
        >
          <Table.ColumnGroup>
            {visibleColumnIds.map((columnId) => (
              <Table.Column key={columnId} style={{ width: `${columnWidths[columnId]}px` }} />
            ))}
          </Table.ColumnGroup>
          <Table.Header>
            <Table.Row>
              {headers.map((column) => {
                const isSorted = tableSettings.sortColumnId === column.id
                const sortMark = isSorted ? (tableSettings.sortDescending ? ' ↓' : ' ↑') : ''
                const widthPx = columnWidths[column.id]
                return (
                  <ResizableColumnHeader
                    key={column.id}
                    column={column}
                    widthPx={widthPx}
                    sortMark={sortMark}
                    onSort={() => setSort(column.id)}
                    onResizeStart={startResize}
                  />
                )
              })}
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {sortedTorrents.map((torrent) => (
              <TorrentTableRow
                key={torrent.id}
                torrent={torrent}
                selectedId={selectedId}
                menuRowId={menuRowId}
                visibleColumnIds={visibleColumnIds}
                columnWidths={columnWidths}
                onSelect={handleRowSelect}
                onContextMenu={handleRowContextMenu}
              />
            ))}
          </Table.Body>
        </Table.Root>
      </Table.ScrollArea>
      <FloatingContextMenu
        open={contextTarget !== null && contextMenuItems.length > 0}
        x={contextTarget?.x ?? 0}
        y={contextTarget?.y ?? 0}
        items={contextMenuItems}
        onClose={closeContextMenu}
      />
      <ScrollToTopButton visible={showScrollTop} onClick={scrollToTop} />
      {sortedTorrents.length === 0 && (
        <Text py={8} textAlign="center" color="fg.muted">
          No torrents
        </Text>
      )}
      {detailsTorrent && (
        <TorrentDetailsDialog
          torrent={detailsTorrent}
          open={detailsOpen}
          onClose={closeDetails}
          onExited={handleDetailsExited}
          onTorrentsChanged={onTorrentsChanged}
        />
      )}
      <TorrentMoveDialog
        torrent={moveTorrent}
        open={moveTorrent !== null}
        busy={actionBusy}
        directories={directories}
        onClose={() => setMoveTorrent(null)}
        onConfirm={handleMoveConfirm}
      />
      <TorrentRemoveDialog
        torrent={removeTorrent}
        open={removeTorrent !== null}
        busy={actionBusy}
        onClose={() => setRemoveTorrent(null)}
        onConfirm={handleRemoveConfirm}
      />
    </Box>
  )
}

export { ColumnSettingsPanel } from './ColumnSettingsPanel'
