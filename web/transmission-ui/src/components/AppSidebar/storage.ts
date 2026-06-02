import type { SidebarMode } from './types'

const MODE_KEY = 'transmissionnet.sidebar.mode'
const FLOAT_X_KEY = 'transmissionnet.sidebar.floatX'
const FLOAT_Y_KEY = 'transmissionnet.sidebar.floatY'

export function loadSidebarMode(): SidebarMode {
  const raw = localStorage.getItem(MODE_KEY)
  if (raw === 'expanded' || raw === 'collapsed' || raw === 'floating') return raw
  return 'expanded'
}

export function saveSidebarMode(mode: SidebarMode) {
  localStorage.setItem(MODE_KEY, mode)
}

export function loadFloatPosition(): { x: number; y: number } {
  const x = Number(localStorage.getItem(FLOAT_X_KEY))
  const y = Number(localStorage.getItem(FLOAT_Y_KEY))
  if (Number.isFinite(x) && Number.isFinite(y)) return { x, y }
  return { x: 16, y: 72 }
}

export function saveFloatPosition(x: number, y: number) {
  localStorage.setItem(FLOAT_X_KEY, String(Math.round(x)))
  localStorage.setItem(FLOAT_Y_KEY, String(Math.round(y)))
}
