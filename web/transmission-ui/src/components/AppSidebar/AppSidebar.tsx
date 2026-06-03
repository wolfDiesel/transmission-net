import { Box, Flex, Portal } from '@chakra-ui/react'
import { useCallback, useEffect, useRef, useState, type PointerEvent as ReactPointerEvent } from 'react'
import { NAV_ITEMS } from './navItems'
import {
  ChevronLeftIcon,
  ChevronRightIcon,
  FloatIcon,
} from './SidebarIcons'
import {
  FLOATING_LAUNCHER_SIZE,
  FloatingMenuIslands,
  FloatingMenuLauncher,
} from './FloatingMenuIslands'
import { islandPanelStyle } from '../layout/islandStyles'
import { SidebarControl, SidebarControlsGroup } from './SidebarControl'
import { SidebarNavItem } from './SidebarNavItem'
import { loadSidebarMode, saveFloatPosition, saveSidebarMode } from './storage'
import type { SidebarMode } from './types'
import { useDraggable } from './useDraggable'
import { AppLogo, AppLogoMark } from '../brand/AppLogo'

const EXPANDED_WIDTH = 220
const COLLAPSED_WIDTH = 64
export function AppSidebar() {
  const [mode, setMode] = useState<SidebarMode>(loadSidebarMode)
  const [menuOpen, setMenuOpen] = useState(false)
  const { position, dragHandlers, wasDragged, clampToViewport } = useDraggable()
  const menuRef = useRef<HTMLDivElement>(null)

  const setModePersisted = useCallback((next: SidebarMode) => {
    setMode(next)
    saveSidebarMode(next)
    if (next !== 'floating') {
      setMenuOpen(false)
    }
  }, [])

  useEffect(() => {
    if (mode !== 'floating') return
    const onResize = () => clampToViewport()
    window.addEventListener('resize', onResize)
    return () => window.removeEventListener('resize', onResize)
  }, [mode, clampToViewport])

  useEffect(() => {
    if (!menuOpen) return

    const onPointerDown = (e: PointerEvent) => {
      const target = e.target as Node
      if (menuRef.current?.contains(target)) return
      setMenuOpen(false)
    }

    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setMenuOpen(false)
    }

    window.addEventListener('pointerdown', onPointerDown)
    window.addEventListener('keydown', onKeyDown)
    return () => {
      window.removeEventListener('pointerdown', onPointerDown)
      window.removeEventListener('keydown', onKeyDown)
    }
  }, [menuOpen])

  const dockedWidth = mode === 'expanded' ? EXPANDED_WIDTH : mode === 'collapsed' ? COLLAPSED_WIDTH : 0

  const handleFloatLauncherPointerUp = (e: ReactPointerEvent<HTMLElement>) => {
    dragHandlers.onPointerUp(e)
    if (!wasDragged()) {
      setMenuOpen((open) => !open)
    }
  }

  const closeMenu = () => setMenuOpen(false)

  if (mode === 'floating') {
    return (
      <>
        <FloatingMenuLauncher
          left={position.x}
          top={position.y}
          dragHandlers={dragHandlers}
          onPointerUp={handleFloatLauncherPointerUp}
        >
          <AppLogoMark size={28} />
        </FloatingMenuLauncher>

        {menuOpen && (
          <Portal>
            <FloatingMenuIslands
              menuRef={menuRef}
              anchorX={position.x}
              anchorY={position.y}
              launcherSize={FLOATING_LAUNCHER_SIZE}
              onNavigate={closeMenu}
              onDock={() => setModePersisted('expanded')}
            />
          </Portal>
        )}
      </>
    )
  }

  return (
    <Box
      as="nav"
      w={`${dockedWidth}px`}
      flexShrink={0}
      alignSelf="stretch"
      py={4}
      px={mode === 'collapsed' ? 2 : 3}
      display="flex"
      flexDirection="column"
      transition="width 0.2s ease"
      overflow="hidden"
      {...islandPanelStyle}
    >
      <Box px={mode === 'expanded' ? 2 : 0} mb={5} display="flex" justifyContent={mode === 'expanded' ? 'flex-start' : 'center'}>
        {mode === 'expanded' ? (
          <AppLogo size={36} showLabel labelSize="sm" />
        ) : (
          <AppLogoMark size={32} />
        )}
      </Box>

      <Flex direction="column" flex="1" gap={1} px={mode === 'collapsed' ? 2 : 0}>
        {NAV_ITEMS.map((item) => (
          <SidebarNavItem key={item.to} item={item} compact={mode === 'collapsed'} />
        ))}
      </Flex>

      <SidebarControlsGroup>
        {mode === 'expanded' ? (
          <Flex gap={2}>
            <SidebarControl
              label="Collapse"
              showLabel
              onClick={() => setModePersisted('collapsed')}
            >
              <ChevronLeftIcon />
            </SidebarControl>
            <SidebarControl
              label="Float"
              showLabel
              onClick={() => {
                saveFloatPosition(position.x, position.y)
                setModePersisted('floating')
              }}
            >
              <FloatIcon />
            </SidebarControl>
          </Flex>
        ) : (
          <>
            <SidebarControl label="Expand" onClick={() => setModePersisted('expanded')}>
              <ChevronRightIcon />
            </SidebarControl>
            <SidebarControl
              label="Float"
              onClick={() => {
                saveFloatPosition(position.x, position.y)
                setModePersisted('floating')
              }}
            >
              <FloatIcon />
            </SidebarControl>
          </>
        )}
      </SidebarControlsGroup>
    </Box>
  )
}
