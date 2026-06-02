import { Box, Flex, Menu, Portal } from '@chakra-ui/react'
import type { ReactNode } from 'react'
import type { ContextMenuItem, ContextMenuItemAction } from './types'
import {
  contextMenuContentProps,
  contextMenuDangerItemProps,
  contextMenuItemProps,
} from './menuItemStyles'

export type ContextMenuProps = {
  items: ContextMenuItem[]
  children: ReactNode
  minW?: string
  elevated?: boolean
  onOpenChange?: (open: boolean) => void
}

const elevatedLayerStyle = {
  zIndex: 'max',
} as const

function renderActionItem(item: ContextMenuItemAction) {
  const props = item.tone === 'danger' ? contextMenuDangerItemProps : contextMenuItemProps

  return (
    <Menu.Item
      key={item.id}
      value={item.id}
      disabled={item.disabled}
      onSelect={item.onSelect}
      {...props}
    >
      {item.label}
    </Menu.Item>
  )
}

function renderSubmenu(item: Extract<ContextMenuItem, { type: 'submenu' }>) {
  return (
    <Menu.Root key={item.id} positioning={{ placement: 'right-start', gutter: 4 }}>
      <Menu.TriggerItem w="full" {...contextMenuItemProps}>
        <Flex w="full" align="center" justify="space-between" gap={3}>
          <Menu.ItemText>{item.label}</Menu.ItemText>
          <Box as="span" color="fg.muted" fontSize="sm" flexShrink={0} aria-hidden>
            ›
          </Box>
        </Flex>
      </Menu.TriggerItem>
      <Portal>
        <Menu.Positioner>
          <Menu.Content {...contextMenuContentProps} minW="140px">
            {item.items.map(renderActionItem)}
          </Menu.Content>
        </Menu.Positioner>
      </Portal>
    </Menu.Root>
  )
}

function renderItem(item: ContextMenuItem, index: number) {
  if (item.type === 'separator') {
    return <Menu.Separator key={item.id ?? `sep-${index}`} my={1} borderColor="border" />
  }
  if (item.type === 'submenu') return renderSubmenu(item)
  return renderActionItem(item)
}

export function ContextMenu({
  items,
  children,
  minW = '200px',
  elevated = false,
  onOpenChange,
}: ContextMenuProps) {
  const layerStyle = elevated ? elevatedLayerStyle : undefined

  return (
    <Menu.Root onOpenChange={(details) => onOpenChange?.(details.open)}>
      <Menu.ContextTrigger asChild>{children}</Menu.ContextTrigger>
      <Portal>
        <Menu.Positioner {...layerStyle}>
          <Menu.Content {...contextMenuContentProps} {...layerStyle} minW={minW}>
            {items.map(renderItem)}
          </Menu.Content>
        </Menu.Positioner>
      </Portal>
    </Menu.Root>
  )
}
