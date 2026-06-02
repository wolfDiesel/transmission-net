import { useCallback, useEffect, useRef, useState } from 'react'
import { clampColumnWidth } from '../../features/torrentTable/columnWidths'

type ResizeState = {
  columnId: string
  startX: number
  startWidth: number
}

export function useColumnResize(onCommit: (columnId: string, widthPx: number) => void) {
  const resizeRef = useRef<ResizeState | null>(null)
  const currentWidthRef = useRef(0)
  const [previewWidths, setPreviewWidths] = useState<Record<string, number> | null>(null)

  const endResize = useCallback(() => {
    const state = resizeRef.current
    resizeRef.current = null
    setPreviewWidths(null)
    if (state) {
      onCommit(state.columnId, clampColumnWidth(currentWidthRef.current || state.startWidth))
    }
  }, [onCommit])

  useEffect(() => {
    const onPointerMove = (e: PointerEvent) => {
      const state = resizeRef.current
      if (!state) return
      const next = clampColumnWidth(state.startWidth + (e.clientX - state.startX))
      currentWidthRef.current = next
      setPreviewWidths({ [state.columnId]: next })
    }

    const onPointerUp = () => endResize()

    window.addEventListener('pointermove', onPointerMove)
    window.addEventListener('pointerup', onPointerUp)
    window.addEventListener('pointercancel', onPointerUp)
    return () => {
      window.removeEventListener('pointermove', onPointerMove)
      window.removeEventListener('pointerup', onPointerUp)
      window.removeEventListener('pointercancel', onPointerUp)
    }
  }, [endResize])

  const startResize = useCallback((columnId: string, startWidth: number, clientX: number) => {
    const width = clampColumnWidth(startWidth)
    resizeRef.current = { columnId, startX: clientX, startWidth: width }
    currentWidthRef.current = width
    setPreviewWidths({ [columnId]: width })
  }, [])

  return { previewWidths, startResize }
}
