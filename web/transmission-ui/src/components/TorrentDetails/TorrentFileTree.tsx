import { Box, Flex, Text } from '@chakra-ui/react'
import { useCallback, useMemo, useState, type MouseEvent, type RefObject } from 'react'
import type { TorrentFileNodeDto } from '../../api/types'
import { TORRENT_ROOT_SCOPE } from '../../features/torrentMassRename'
import { FloatingContextMenu } from '../ui/FloatingContextMenu'
import { useI18n } from '../../i18n'
import { formatPercent, formatSize } from '../../utils/format'

type TorrentFileTreeProps = {
  nodes: TorrentFileNodeDto[]
  torrentName?: string
  onRename?: (node: TorrentFileNodeDto) => void
  onMassRename?: (scopePath: string) => void
  menuPortalContainer?: RefObject<HTMLElement | null>
}

type ContextTarget = {
  kind: 'root' | 'node'
  node?: TorrentFileNodeDto
  x: number
  y: number
}

export function TorrentFileTree({
  nodes,
  torrentName,
  onRename,
  onMassRename,
  menuPortalContainer,
}: TorrentFileTreeProps) {
  const { t } = useI18n()
  const defaultExpanded = useMemo(() => collectFolderPaths(nodes, 2), [nodes])
  const [expanded, setExpanded] = useState<Set<string>>(() => defaultExpanded)
  const [contextTarget, setContextTarget] = useState<ContextTarget | null>(null)

  const toggle = useCallback((path: string) => {
    setExpanded((prev) => {
      const next = new Set(prev)
      if (next.has(path)) next.delete(path)
      else next.add(path)
      return next
    })
  }, [])

  const openNodeContextMenu = useCallback(
    (node: TorrentFileNodeDto, event: MouseEvent) => {
      if (!onRename && !onMassRename) return
      event.preventDefault()
      event.stopPropagation()
      setContextTarget({
        kind: 'node',
        node,
        x: event.clientX,
        y: event.clientY,
      })
    },
    [onRename, onMassRename],
  )

  const openRootContextMenu = useCallback(
    (event: MouseEvent) => {
      if (!onMassRename) return
      event.preventDefault()
      event.stopPropagation()
      setContextTarget({
        kind: 'root',
        x: event.clientX,
        y: event.clientY,
      })
    },
    [onMassRename],
  )

  const menuItems = useMemo(() => {
    if (!contextTarget) return []

    if (contextTarget.kind === 'root') {
      return onMassRename
        ? [
            {
              id: 'mass-rename',
              label: t('torrentDetails.fileTree.massRename'),
              onSelect: () => onMassRename(TORRENT_ROOT_SCOPE),
            },
          ]
        : []
    }

    const node = contextTarget.node
    if (!node) return []
    const items: { id: string; label: string; onSelect: () => void }[] = []

    if (onRename) {
      items.push({
        id: 'rename',
        label: t('torrentDetails.fileTree.rename'),
        onSelect: () => onRename(node),
      })
    }

    if (onMassRename && node.isFolder) {
      items.push({
        id: 'mass-rename',
        label: t('torrentDetails.fileTree.massRename'),
        onSelect: () => onMassRename(node.path),
      })
    }

    return items
  }, [contextTarget, onMassRename, onRename, t])

  const menuX = contextTarget?.x ?? 0
  const menuY = contextTarget?.y ?? 0

  if (nodes.length === 0) {
    return (
      <Text py={6} textAlign="center" color="fg.muted" fontSize="sm">
        No files
      </Text>
    )
  }

  return (
    <>
      <Flex
        align="center"
        gap={2}
        px={2}
        py={2}
        mb={1}
        borderRadius="sm"
        bg="bg.emphasized"
        borderWidth="1px"
        borderColor="border"
        cursor={onMassRename ? 'context-menu' : 'default'}
        onContextMenu={openRootContextMenu}
      >
        <Text fontSize="sm" color="fg.muted" flex="1" truncate title={torrentName}>
          {torrentName ? `${torrentName} (all files)` : 'All files'}
        </Text>
        {onMassRename && (
          <Text fontSize="xs" color="fg.subtle">
            right-click
          </Text>
        )}
      </Flex>

      <Box fontSize="sm">
        {nodes.map((node) => (
          <TorrentFileTreeNode
            key={node.path}
            node={node}
            depth={0}
            expanded={expanded}
            onToggle={toggle}
            onContextMenu={openNodeContextMenu}
          />
        ))}
      </Box>

      <FloatingContextMenu
        open={contextTarget !== null && menuItems.length > 0}
        x={menuX}
        y={menuY}
        portalContainer={menuPortalContainer}
        onClose={() => setContextTarget(null)}
        items={menuItems}
      />
    </>
  )
}

type TorrentFileTreeNodeProps = {
  node: TorrentFileNodeDto
  depth: number
  expanded: Set<string>
  onToggle: (path: string) => void
  onContextMenu: (node: TorrentFileNodeDto, event: MouseEvent) => void
}

function TorrentFileTreeNode({
  node,
  depth,
  expanded,
  onToggle,
  onContextMenu,
}: TorrentFileTreeNodeProps) {
  const isOpen = expanded.has(node.path)
  const progress = node.length > 0 ? node.bytesCompleted / node.length : 0

  return (
    <Box>
      <Flex
        w="full"
        align="center"
        gap={2}
        py={1.5}
        pl={`${depth * 16 + 8}px`}
        pr={2}
        borderRadius="sm"
        _hover={{ bg: 'bg.muted' }}
        cursor={node.isFolder ? 'pointer' : 'default'}
        onClick={() => {
          if (node.isFolder) onToggle(node.path)
        }}
        onContextMenu={(event) => onContextMenu(node, event)}
      >
        {node.isFolder ? (
          <Text w="14px" flexShrink={0} color="fg.muted" fontSize="xs">
            {isOpen ? '▾' : '▸'}
          </Text>
        ) : (
          <Box w="14px" flexShrink={0} />
        )}
        <Text flex="1" truncate title={node.path} color="fg">
          {node.name}
        </Text>
        <Text flexShrink={0} color="fg.muted" fontSize="xs">
          {formatSize(node.length)}
        </Text>
        <Text w="52px" flexShrink={0} textAlign="right" color="brand.500" fontSize="xs">
          {formatPercent(progress)}
        </Text>
        {!node.isFolder && node.wanted === false && (
          <Text flexShrink={0} fontSize="xs" color="fg.subtle">
            skip
          </Text>
        )}
      </Flex>
      {node.isFolder &&
        isOpen &&
        node.children.map((child) => (
          <TorrentFileTreeNode
            key={child.path}
            node={child}
            depth={depth + 1}
            expanded={expanded}
            onToggle={onToggle}
            onContextMenu={onContextMenu}
          />
        ))}
    </Box>
  )
}

function collectFolderPaths(nodes: TorrentFileNodeDto[], maxDepth: number, depth = 0): Set<string> {
  const paths = new Set<string>()
  if (depth >= maxDepth) return paths

  for (const node of nodes) {
    if (!node.isFolder) continue
    paths.add(node.path)
    foreachChildPath(node.children, maxDepth, depth + 1, paths)
  }

  return paths
}

function foreachChildPath(
  nodes: TorrentFileNodeDto[],
  maxDepth: number,
  depth: number,
  paths: Set<string>,
) {
  if (depth >= maxDepth) return

  for (const node of nodes) {
    if (!node.isFolder) continue
    paths.add(node.path)
    foreachChildPath(node.children, maxDepth, depth + 1, paths)
  }
}
