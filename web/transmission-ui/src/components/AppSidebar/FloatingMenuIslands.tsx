import { Box, Flex, Text } from '@chakra-ui/react'
import type { ReactNode, RefObject } from 'react'
import { NavLink } from 'react-router-dom'
import { navIconForPath, PinIcon } from './SidebarIcons'
import { NAV_ITEMS } from './navItems'
import type { NavItemConfig } from './types'

const ISLAND_SIZE = 44
const ISLAND_GAP = 8
const ROW_GAP = 10

const islandShadow = '0 8px 24px rgba(0, 0, 0, 0.55)'

type IslandTone = 'idle' | 'active'

function islandTone(isActive: boolean): IslandTone {
  return isActive ? 'active' : 'idle'
}

function islandColors(tone: IslandTone) {
  if (tone === 'active') {
    return {
      bg: 'bg.emphasized',
      borderColor: 'brand.500',
      color: 'brand.500',
    }
  }
  return {
    bg: 'surface.panel',
    borderColor: 'border',
    color: 'fg.muted',
  }
}

function IslandIcon({ children, tone }: { children: ReactNode; tone: IslandTone }) {
  const colors = islandColors(tone)
  return (
    <Box
      w={`${ISLAND_SIZE}px`}
      h={`${ISLAND_SIZE}px`}
      flexShrink={0}
      display="flex"
      alignItems="center"
      justifyContent="center"
      borderRadius="full"
      borderWidth="1px"
      boxShadow={islandShadow}
      transition="all 0.15s"
      _groupHover={hoverIsland}
      {...colors}
    >
      {children}
    </Box>
  )
}

function IslandLabel({ children, tone }: { children: ReactNode; tone: IslandTone }) {
  const colors = islandColors(tone)
  return (
    <Box
      h={`${ISLAND_SIZE}px`}
      display="flex"
      alignItems="center"
      px={4}
      borderRadius="full"
      borderWidth="1px"
      boxShadow={islandShadow}
      whiteSpace="nowrap"
      transition="all 0.15s"
      _groupHover={hoverIsland}
      {...colors}
    >
      <Text fontSize="sm" fontWeight={tone === 'active' ? 'semibold' : 'medium'}>
        {children}
      </Text>
    </Box>
  )
}

type FloatingMenuRowProps = {
  item: NavItemConfig
  onNavigate?: () => void
}

const hoverIsland = {
  borderColor: 'brand.500',
  color: 'brand.500',
  bg: 'bg.emphasized',
}

function FloatingMenuRow({ item, onNavigate }: FloatingMenuRowProps) {
  return (
    <NavLink to={item.to} end={item.end} style={{ textDecoration: 'none' }} onClick={onNavigate}>
      {({ isActive }) => {
        const tone = islandTone(isActive)
        return (
          <Flex align="center" gap={`${ISLAND_GAP}px`} role="group">
            <IslandIcon tone={tone}>{navIconForPath(item.to, 22)}</IslandIcon>
            <IslandLabel tone={tone}>{item.label}</IslandLabel>
          </Flex>
        )
      }}
    </NavLink>
  )
}

type FloatingDockRowProps = {
  onDock: () => void
}

function FloatingDockRow({ onDock }: FloatingDockRowProps) {
  const tone: IslandTone = 'idle'
  return (
    <Flex
      align="center"
      gap={`${ISLAND_GAP}px`}
      cursor="pointer"
      onClick={onDock}
      role="group"
      tabIndex={0}
      onKeyDown={(e) => {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault()
          onDock()
        }
      }}
    >
      <IslandIcon tone={tone}>
        <PinIcon size={20} />
      </IslandIcon>
      <IslandLabel tone={tone}>Dock</IslandLabel>
    </Flex>
  )
}

type FloatingMenuIslandsProps = {
  menuRef: RefObject<HTMLDivElement | null>
  anchorX: number
  anchorY: number
  launcherSize: number
  onNavigate: () => void
  onDock: () => void
}

export function FloatingMenuIslands({
  menuRef,
  anchorX,
  anchorY,
  launcherSize,
  onNavigate,
  onDock,
}: FloatingMenuIslandsProps) {
  const rowWidth = ISLAND_SIZE + ISLAND_GAP + 120
  const menuLeft = Math.min(
    anchorX + launcherSize + 12,
    Math.max(8, window.innerWidth - rowWidth - 8),
  )
  const rowCount = NAV_ITEMS.length + 1
  const menuHeight = rowCount * ISLAND_SIZE + (rowCount - 1) * ROW_GAP
  const menuTop = Math.min(anchorY, Math.max(8, window.innerHeight - menuHeight - 8))

  return (
    <Box
      ref={menuRef}
      position="fixed"
      left={`${menuLeft}px`}
      top={`${menuTop}px`}
      zIndex={1401}
      display="flex"
      flexDirection="column"
      gap={`${ROW_GAP}px`}
    >
      {NAV_ITEMS.map((item) => (
        <FloatingMenuRow key={item.to} item={item} onNavigate={onNavigate} />
      ))}
      <FloatingDockRow onDock={onDock} />
    </Box>
  )
}

export const FLOATING_LAUNCHER_SIZE = 52

export function FloatingMenuLauncher({
  left,
  top,
  children,
  dragHandlers,
  onPointerUp,
}: {
  left: number
  top: number
  children: ReactNode
  dragHandlers: {
    onPointerDown: (e: React.PointerEvent<HTMLElement>) => void
    onPointerMove: (e: React.PointerEvent<HTMLElement>) => void
  }
  onPointerUp: (e: React.PointerEvent<HTMLElement>) => void
}) {
  return (
    <Box
      position="fixed"
      left={`${left}px`}
      top={`${top}px`}
      zIndex={1400}
      w={`${FLOATING_LAUNCHER_SIZE}px`}
      h={`${FLOATING_LAUNCHER_SIZE}px`}
      borderRadius="full"
      bg="surface.panel"
      borderWidth="2px"
      borderColor="brand.500"
      boxShadow={islandShadow}
      display="flex"
      alignItems="center"
      justifyContent="center"
      color="brand.500"
      cursor="grab"
      touchAction="none"
      userSelect="none"
      _active={{ cursor: 'grabbing' }}
      aria-label="Navigation menu"
      title="Drag to move · Click to open menu"
      onPointerDown={dragHandlers.onPointerDown}
      onPointerMove={dragHandlers.onPointerMove}
      onPointerUp={onPointerUp}
    >
      {children}
    </Box>
  )
}
