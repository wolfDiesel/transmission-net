import { Box, Table } from '@chakra-ui/react'
import type { PointerEvent } from 'react'
import type { TorrentColumnDef } from '../../features/torrentTable/columns'

const headerStyle = {
  bg: 'transparent',
  color: 'fg.muted',
  fontWeight: 'medium',
  fontSize: 'xs',
  textTransform: 'none' as const,
  letterSpacing: 'normal',
  borderBottomWidth: '1px',
  borderBottomColor: 'border.muted',
  py: 2,
  top: 0,
  zIndex: 2,
  userSelect: 'none' as const,
  overflow: 'hidden',
  whiteSpace: 'nowrap' as const,
}

type ResizableColumnHeaderProps = {
  column: TorrentColumnDef
  widthPx: number
  sortMark: string
  onSort: () => void
  onResizeStart: (columnId: string, widthPx: number, clientX: number) => void
}

export function ResizableColumnHeader({
  column,
  widthPx,
  sortMark,
  onSort,
  onResizeStart,
}: ResizableColumnHeaderProps) {
  const handleResizePointerDown = (e: PointerEvent<HTMLDivElement>) => {
    e.preventDefault()
    e.stopPropagation()
    e.currentTarget.setPointerCapture(e.pointerId)
    onResizeStart(column.id, widthPx, e.clientX)
  }

  return (
    <Table.ColumnHeader
      position="sticky"
      {...headerStyle}
      w={`${widthPx}px`}
      minW={`${widthPx}px`}
      maxW={`${widthPx}px`}
      textAlign={column.align}
      cursor={column.sortable !== false ? 'pointer' : 'default'}
      onClick={() => {
        if (column.sortable !== false) onSort()
      }}
    >
      <Box truncate pr={2}>
        {column.label}
        {sortMark}
      </Box>
      <Box
        position="absolute"
        top={0}
        right={0}
        h="100%"
        w="8px"
        cursor="col-resize"
        touchAction="none"
        zIndex={3}
        _hover={{ bg: 'border.emphasized', opacity: 0.6 }}
        onPointerDown={handleResizePointerDown}
        onClick={(e) => e.stopPropagation()}
      />
    </Table.ColumnHeader>
  )
}
