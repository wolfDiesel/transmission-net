import { useCallback, useRef, useState, type PointerEvent as ReactPointerEvent } from 'react'
import { loadFloatPosition, saveFloatPosition } from './storage'

const DRAG_THRESHOLD_PX = 6
const FLOAT_SIZE = 52

function clampPosition(x: number, y: number) {
  const maxX = Math.max(8, window.innerWidth - FLOAT_SIZE - 8)
  const maxY = Math.max(8, window.innerHeight - FLOAT_SIZE - 8)
  return {
    x: Math.min(Math.max(8, x), maxX),
    y: Math.min(Math.max(8, y), maxY),
  }
}

export function useDraggable() {
  const [position, setPosition] = useState(loadFloatPosition)
  const dragState = useRef({
    active: false,
    moved: false,
    offsetX: 0,
    offsetY: 0,
    startX: 0,
    startY: 0,
  })

  const onPointerDown = useCallback((e: ReactPointerEvent<HTMLElement>) => {
    if (e.button !== 0) return
    const target = e.currentTarget
    target.setPointerCapture(e.pointerId)
    dragState.current = {
      active: true,
      moved: false,
      offsetX: e.clientX - position.x,
      offsetY: e.clientY - position.y,
      startX: e.clientX,
      startY: e.clientY,
    }
  }, [position.x, position.y])

  const onPointerMove = useCallback((e: ReactPointerEvent<HTMLElement>) => {
    const state = dragState.current
    if (!state.active) return

    const dx = e.clientX - state.startX
    const dy = e.clientY - state.startY
    if (!state.moved && Math.hypot(dx, dy) >= DRAG_THRESHOLD_PX) {
      state.moved = true
    }

    if (state.moved) {
      const next = clampPosition(e.clientX - state.offsetX, e.clientY - state.offsetY)
      setPosition(next)
    }
  }, [])

  const onPointerUp = useCallback((e: ReactPointerEvent<HTMLElement>) => {
    const state = dragState.current
    if (!state.active) return

    state.active = false
    e.currentTarget.releasePointerCapture(e.pointerId)

    if (state.moved) {
      const next = clampPosition(e.clientX - state.offsetX, e.clientY - state.offsetY)
      setPosition(next)
      saveFloatPosition(next.x, next.y)
    }
  }, [])

  const wasDragged = useCallback(() => {
    const moved = dragState.current.moved
    dragState.current.moved = false
    return moved
  }, [])

  const clampToViewport = useCallback(() => {
    setPosition((current) => {
      const next = clampPosition(current.x, current.y)
      saveFloatPosition(next.x, next.y)
      return next
    })
  }, [])

  return {
    position,
    dragHandlers: { onPointerDown, onPointerMove, onPointerUp },
    wasDragged,
    clampToViewport,
  }
}
