import { Box, Flex } from '@chakra-ui/react'
import { useCallback, useEffect, useRef, useState, type ReactNode } from 'react'

type ResizableSplitPaneProps = {
  left: ReactNode
  right: ReactNode
  defaultLeftRatio?: number
  minLeftRatio?: number
  minRightRatio?: number
}

export function ResizableSplitPane({
  left,
  right,
  defaultLeftRatio = 2 / 3,
  minLeftRatio = 1 / 3,
  minRightRatio = 1 / 3,
}: ResizableSplitPaneProps) {
  const containerRef = useRef<HTMLDivElement>(null)
  const [leftRatio, setLeftRatio] = useState(defaultLeftRatio)
  const dragRef = useRef<{ startX: number; startRatio: number } | null>(null)

  const clampRatio = useCallback(
    (ratio: number) => {
      const maxLeft = 1 - minRightRatio
      const minLeft = minLeftRatio
      return Math.min(maxLeft, Math.max(minLeft, ratio))
    },
    [minLeftRatio, minRightRatio],
  )

  useEffect(() => {
    const onPointerMove = (e: PointerEvent) => {
      const drag = dragRef.current
      const container = containerRef.current
      if (!drag || !container) return

      const width = container.getBoundingClientRect().width
      if (width <= 0) return

      const delta = (e.clientX - drag.startX) / width
      setLeftRatio(clampRatio(drag.startRatio + delta))
    }

    const endDrag = () => {
      dragRef.current = null
    }

    window.addEventListener('pointermove', onPointerMove)
    window.addEventListener('pointerup', endDrag)
    window.addEventListener('pointercancel', endDrag)
    return () => {
      window.removeEventListener('pointermove', onPointerMove)
      window.removeEventListener('pointerup', endDrag)
      window.removeEventListener('pointercancel', endDrag)
    }
  }, [clampRatio])

  const startDrag = (e: React.PointerEvent<HTMLDivElement>) => {
    e.preventDefault()
    e.stopPropagation()
    dragRef.current = { startX: e.clientX, startRatio: leftRatio }
    e.currentTarget.setPointerCapture(e.pointerId)
  }

  const leftPercent = `${leftRatio * 100}%`

  return (
    <Flex ref={containerRef} flex="1" minH={0} minW={0} direction="row" overflow="hidden">
      <Box
        flex={`0 0 ${leftPercent}`}
        minW={0}
        maxW={`${(1 - minRightRatio) * 100}%`}
        overflow="auto"
      >
        {left}
      </Box>
      <Box
        role="separator"
        aria-orientation="vertical"
        aria-valuenow={Math.round(leftRatio * 100)}
        flexShrink={0}
        w="6px"
        cursor="col-resize"
        bg="border"
        transition="background-color 0.12s"
        _hover={{ bg: 'brand.500' }}
        _active={{ bg: 'brand.600' }}
        onPointerDown={startDrag}
      />
      <Box flex="1" minW={`${minRightRatio * 100}%`} overflow="hidden" display="flex" flexDirection="column">
        {right}
      </Box>
    </Flex>
  )
}
