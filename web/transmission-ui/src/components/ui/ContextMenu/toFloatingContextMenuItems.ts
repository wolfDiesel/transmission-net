import type { FloatingContextMenuItem } from '../FloatingContextMenu'
import type { ContextMenuItem } from './types'

export function toFloatingContextMenuItems(
  items: ContextMenuItem[],
): FloatingContextMenuItem[] {
  const result: FloatingContextMenuItem[] = []

  for (const item of items) {
    if (item.type === 'separator') {
      result.push({ id: item.id ?? `sep-${result.length}`, kind: 'separator' })
      continue
    }

    if (item.type === 'submenu') {
      result.push({
        id: item.id,
        kind: 'submenu',
        label: item.label,
        submenuItems: item.items.map((sub) => ({
          id: sub.id,
          kind: 'action',
          label: String(sub.label),
          onSelect: sub.onSelect,
          disabled: sub.disabled,
        })),
      })
      continue
    }

    result.push({
      id: item.id,
      kind: 'action',
      label: String(item.label),
      onSelect: item.onSelect,
      disabled: item.disabled,
      tone: item.tone,
    })
  }

  return result
}
