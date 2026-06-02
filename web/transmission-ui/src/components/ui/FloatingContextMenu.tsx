import { Box, Flex, Text } from '@chakra-ui/react'
import { useEffect, useRef, useState, type RefObject } from 'react'
import { createPortal } from 'react-dom'
import { getOverlayRoot } from '../../lib/overlayRoot'
import { contextMenuContentProps } from './ContextMenu/menuItemStyles'

const menuItemHighlightBg =
  'color-mix(in srgb, var(--app-brand-500, #F07818) 14%, var(--chakra-colors-surface-raised, #1A1A1A))'

export type FloatingContextMenuItem = {
  id: string
  kind?: 'action' | 'separator' | 'submenu'
  label?: string
  onSelect?: () => void
  disabled?: boolean
  tone?: 'default' | 'danger'
  submenuItems?: FloatingContextMenuItem[]
}

type FloatingContextMenuProps = {
  open: boolean
  x: number
  y: number
  items: FloatingContextMenuItem[]
  onClose: () => void
  portalContainer?: RefObject<HTMLElement | null>
}

function MenuActionButton({
  item,
  onClose,
}: {
  item: FloatingContextMenuItem
  onClose: () => void
}) {
  return (
    <Box
      as="button"
      display="flex"
      alignItems="center"
      w="full"
      minH="32px"
      px={3}
      py={2}
      border="none"
      outline="none"
      bg="transparent"
      color={item.tone === 'danger' ? 'red.400' : 'fg'}
      fontSize="sm"
      fontWeight="normal"
      fontFamily="inherit"
      textAlign="left"
      lineHeight="short"
      borderRadius="sm"
      cursor={item.disabled ? 'not-allowed' : 'pointer'}
      opacity={item.disabled ? 0.5 : 1}
      aria-disabled={item.disabled || undefined}
      transition="background-color 0.12s, color 0.12s"
      _hover={
        item.disabled ? undefined : { bg: menuItemHighlightBg, color: 'brand.500' }
      }
      onClick={(e) => {
        e.stopPropagation()
        if (item.disabled || !item.onSelect) return
        item.onSelect()
        onClose()
      }}
    >
      {item.label}
    </Box>
  )
}

function SubmenuRow({
  item,
  onClose,
}: {
  item: FloatingContextMenuItem
  onClose: () => void
}) {
  const [open, setOpen] = useState(false)
  const leaveTimer = useRef<number | undefined>(undefined)

  const clearLeaveTimer = () => {
    if (leaveTimer.current !== undefined) {
      window.clearTimeout(leaveTimer.current)
      leaveTimer.current = undefined
    }
  }

  const scheduleClose = () => {
    clearLeaveTimer()
    leaveTimer.current = window.setTimeout(() => setOpen(false), 120)
  }

  const handleEnter = () => {
    clearLeaveTimer()
    setOpen(true)
  }

  useEffect(() => () => clearLeaveTimer(), [])

  const subItems = item.submenuItems ?? []

  return (
    <Box
      position="relative"
      onMouseEnter={handleEnter}
      onMouseLeave={scheduleClose}
    >
      <Flex
        as="button"
        align="center"
        justify="space-between"
        gap={3}
        w="full"
        minH="32px"
        px={3}
        py={2}
        border="none"
        outline="none"
        bg={open ? menuItemHighlightBg : 'transparent'}
        color={open ? 'brand.500' : 'fg'}
        fontSize="sm"
        fontWeight="normal"
        fontFamily="inherit"
        textAlign="left"
        borderRadius="sm"
        cursor="default"
        transition="background-color 0.12s, color 0.12s"
      >
        <Text as="span">{item.label}</Text>
        <Text as="span" color="fg.muted" fontSize="sm" aria-hidden>
          ›
        </Text>
      </Flex>

      {open && subItems.length > 0 && (
        <Box
          position="absolute"
          left="100%"
          top={0}
          ml={1}
          minW="140px"
          zIndex={2}
          onMouseEnter={handleEnter}
          onMouseLeave={scheduleClose}
          {...contextMenuContentProps}
        >
          {subItems.map((sub) => (
            <MenuActionButton key={sub.id} item={sub} onClose={onClose} />
          ))}
        </Box>
      )}
    </Box>
  )
}

export function FloatingContextMenu({
  open,
  x,
  y,
  items,
  onClose,
  portalContainer,
}: FloatingContextMenuProps) {
  const menuRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!open) return

    const handlePointerDown = (event: PointerEvent) => {
      if (menuRef.current?.contains(event.target as Node)) return
      onClose()
    }

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') onClose()
    }

    window.addEventListener('pointerdown', handlePointerDown, true)
    window.addEventListener('keydown', handleKeyDown)
    return () => {
      window.removeEventListener('pointerdown', handlePointerDown, true)
      window.removeEventListener('keydown', handleKeyDown)
    }
  }, [open, onClose])

  if (!open) return null

  const host = portalContainer?.current ?? getOverlayRoot()

  return createPortal(
    <Box
      ref={menuRef}
      position="fixed"
      left={`${x}px`}
      top={`${y}px`}
      zIndex={1}
      minW="160px"
      pointerEvents="auto"
      onPointerDown={(e) => e.stopPropagation()}
      onClick={(e) => e.stopPropagation()}
      {...contextMenuContentProps}
    >
      {items.map((item) => {
        const kind = item.kind ?? 'action'

        if (kind === 'separator') {
          return (
            <Box
              key={item.id}
              my={1}
              mx={2}
              borderTopWidth="1px"
              borderColor="border"
            />
          )
        }

        if (kind === 'submenu') {
          return <SubmenuRow key={item.id} item={item} onClose={onClose} />
        }

        return <MenuActionButton key={item.id} item={item} onClose={onClose} />
      })}
    </Box>,
    host,
  )
}

export type FloatingContextMenuAction = FloatingContextMenuItem
