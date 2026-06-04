import {
  Box,
  Button,
  Checkbox,
  Flex,
  Popover,
  Portal,
  Text,
} from '@chakra-ui/react'
import { useCallback, useEffect, useRef, useState, type PointerEvent as ReactPointerEvent } from 'react'
import type { TorrentTableSettingsDto } from '../../api/types'
import { useI18n } from '../../i18n'
import { useTorrentColumnDef } from '../../hooks/useTorrentColumnDef'

type ColumnSettingsPanelProps = {
  tableSettings: TorrentTableSettingsDto
  onChange: (columns: TorrentTableSettingsDto['columns']) => void
}

export function ColumnSettingsPanel({ tableSettings, onChange }: ColumnSettingsPanelProps) {
  const { t } = useI18n()
  const getColumnDef = useTorrentColumnDef()
  const listRef = useRef<HTMLDivElement>(null)
  const [draggingId, setDraggingId] = useState<string | null>(null)
  const [hoverTargetId, setHoverTargetId] = useState<string | null>(null)

  const moveColumn = useCallback(
    (fromId: string, toId: string) => {
      if (fromId === toId) return
      const columns = [...tableSettings.columns]
      const fromIndex = columns.findIndex((c) => c.id === fromId)
      const toIndex = columns.findIndex((c) => c.id === toId)
      if (fromIndex < 0 || toIndex < 0) return
      const [item] = columns.splice(fromIndex, 1)
      columns.splice(toIndex, 0, item)
      onChange(columns)
    },
    [onChange, tableSettings.columns],
  )

  const findTargetId = useCallback((clientY: number): string | null => {
    const list = listRef.current
    if (!list) return null

    const rows = list.querySelectorAll<HTMLElement>('[data-column-id]')
    for (const row of rows) {
      const rect = row.getBoundingClientRect()
      if (clientY >= rect.top && clientY <= rect.bottom) {
        return row.dataset.columnId ?? null
      }
    }
    return null
  }, [])

  const toggleVisible = (id: string) => {
    onChange(
      tableSettings.columns.map((c) => (c.id === id ? { ...c, visible: !c.visible } : c)),
    )
  }

  useEffect(() => {
    if (!draggingId) return

    const handlePointerMove = (event: PointerEvent) => {
      const targetId = findTargetId(event.clientY)
      setHoverTargetId(targetId)

      if (targetId && targetId !== draggingId) {
        moveColumn(draggingId, targetId)
      }
    }

    const handlePointerUp = () => {
      setDraggingId(null)
      setHoverTargetId(null)
    }

    window.addEventListener('pointermove', handlePointerMove)
    window.addEventListener('pointerup', handlePointerUp)
    window.addEventListener('pointercancel', handlePointerUp)
    return () => {
      window.removeEventListener('pointermove', handlePointerMove)
      window.removeEventListener('pointerup', handlePointerUp)
      window.removeEventListener('pointercancel', handlePointerUp)
    }
  }, [draggingId, findTargetId, moveColumn])

  const startDrag = (columnId: string, event: ReactPointerEvent<HTMLDivElement>) => {
    event.preventDefault()
    event.stopPropagation()
    event.currentTarget.setPointerCapture(event.pointerId)
    setDraggingId(columnId)
    setHoverTargetId(columnId)
  }

  return (
    <Popover.Root positioning={{ placement: 'bottom-end' }}>
      <Popover.Trigger asChild>
        <Button
          size="sm"
          variant="outline"
          borderColor="border"
          color="fg.muted"
          _hover={{ color: 'brand.500', borderColor: 'brand.500' }}
        >
          {t('torrentsPage.columns')}
        </Button>
      </Popover.Trigger>
      <Portal>
        <Popover.Positioner>
          <Popover.Content
            bg="surface.panel"
            borderColor="border"
            borderWidth="1px"
            minW="280px"
            p={3}
          >
            <Text fontSize="sm" fontWeight="semibold" color="brand.500" mb={2}>
              {t('torrentsPage.columnsPanelTitle')}
            </Text>
            <Text fontSize="xs" color="fg.muted" mb={3}>
              {t('torrentsPage.columnsPanelHint')}
            </Text>
            <Flex ref={listRef} direction="column" gap={1}>
              {tableSettings.columns.map((column) => {
                const def = getColumnDef(column.id)
                const label = def?.label ?? column.id
                const isDragging = draggingId === column.id
                const isHoverTarget = hoverTargetId === column.id && draggingId !== column.id

                return (
                  <Flex
                    key={column.id}
                    data-column-id={column.id}
                    align="center"
                    gap={2}
                    px={2}
                    py={1.5}
                    borderRadius="md"
                    borderWidth="1px"
                    borderColor={
                      isDragging || isHoverTarget ? 'brand.500' : 'border'
                    }
                    bg={isDragging ? 'bg.muted' : 'bg.emphasized'}
                    opacity={isDragging ? 0.85 : 1}
                    transition="border-color 0.12s, background-color 0.12s"
                  >
                    <Box
                      flexShrink={0}
                      color="fg.subtle"
                      fontSize="sm"
                      lineHeight={1}
                      px={1}
                      py={2}
                      cursor={draggingId ? 'grabbing' : 'grab'}
                      touchAction="none"
                      userSelect="none"
                      onPointerDown={(e) => startDrag(column.id, e)}
                    >
                      ⋮⋮
                    </Box>
                    <Checkbox.Root
                      checked={column.visible}
                      onCheckedChange={() => toggleVisible(column.id)}
                      flex="1"
                    >
                      <Checkbox.HiddenInput />
                      <Checkbox.Control borderColor="border" />
                      <Checkbox.Label color="fg" fontSize="sm">
                        {label}
                      </Checkbox.Label>
                    </Checkbox.Root>
                  </Flex>
                )
              })}
            </Flex>
          </Popover.Content>
        </Popover.Positioner>
      </Portal>
    </Popover.Root>
  )
}
