import type { ReactNode } from 'react'

export type ContextMenuItemAction = {
  type: 'item'
  id: string
  label: ReactNode
  onSelect: () => void
  tone?: 'default' | 'danger'
  disabled?: boolean
}

export type ContextMenuItemSubmenu = {
  type: 'submenu'
  id: string
  label: string
  items: ContextMenuItemAction[]
}

export type ContextMenuItemSeparator = {
  type: 'separator'
  id?: string
}

export type ContextMenuItem =
  | ContextMenuItemAction
  | ContextMenuItemSubmenu
  | ContextMenuItemSeparator
