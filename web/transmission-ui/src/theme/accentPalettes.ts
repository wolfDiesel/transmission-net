export const COLOR_SCHEME_IDS = ['orange', 'teal', 'blue', 'purple', 'green'] as const

export type ColorSchemeId = (typeof COLOR_SCHEME_IDS)[number]

export const APPEARANCE_IDS = ['light', 'dark', 'system'] as const

export type AppearanceId = (typeof APPEARANCE_IDS)[number]

export type AccentPalette = {
  id: ColorSchemeId
  label: string
  primary: string
  primaryHover: string
}

const PROGRESS_TRACK_DARK = '#1A1A1A'
const PROGRESS_TRACK_LIGHT = '#E4E4E8'

const PROGRESS_MIX_DARK = 0.52
const PROGRESS_MIX_LIGHT = 0.44

const ROW_SURFACE_DARK = '#1A1A1A'
const ROW_SURFACE_LIGHT = '#F4F4F5'
const ROW_ACTIVE_BASE_DARK = '#141414'
const ROW_ACTIVE_BASE_LIGHT = '#E4E4E8'
const ROW_HOVER_MIX = 0.07
const ROW_ACTIVE_MIX = 0.11

export const ACCENT_PALETTES: AccentPalette[] = [
  { id: 'orange', label: 'Orange', primary: '#F07818', primaryHover: '#E06810' },
  { id: 'teal', label: 'Teal', primary: '#2EB8AA', primaryHover: '#22A89C' },
  { id: 'blue', label: 'Sky blue', primary: '#58A6E8', primaryHover: '#4596D8' },
  { id: 'purple', label: 'Purple', primary: '#A48AF5', primaryHover: '#9378EB' },
  { id: 'green', label: 'Green', primary: '#52C878', primaryHover: '#42B868' },
]

export const DEFAULT_COLOR_SCHEME: ColorSchemeId = 'orange'
export const DEFAULT_APPEARANCE: AppearanceId = 'dark'

export function normalizeColorScheme(value: string | undefined | null): ColorSchemeId {
  if (value && COLOR_SCHEME_IDS.includes(value as ColorSchemeId)) {
    return value as ColorSchemeId
  }
  return DEFAULT_COLOR_SCHEME
}

export function normalizeAppearance(value: string | undefined | null): AppearanceId {
  if (value && APPEARANCE_IDS.includes(value as AppearanceId)) {
    return value as AppearanceId
  }
  return DEFAULT_APPEARANCE
}

export function getAccentPalette(id: ColorSchemeId): AccentPalette {
  return ACCENT_PALETTES.find((p) => p.id === id) ?? ACCENT_PALETTES[0]
}

function progressFill(accent: string, track: string, mixRatio: number): string {
  const percent = Math.round(mixRatio * 100)
  return `color-mix(in srgb, ${accent} ${percent}%, ${track})`
}

function mixHex(accent: string, base: string, accentWeight: number): string {
  const parse = (hex: string) => {
    const h = hex.replace('#', '')
    return [
      Number.parseInt(h.slice(0, 2), 16),
      Number.parseInt(h.slice(2, 4), 16),
      Number.parseInt(h.slice(4, 6), 16),
    ]
  }
  const [ar, ag, ab] = parse(accent)
  const [br, bg, bb] = parse(base)
  const w = accentWeight
  const blend = (a: number, b: number) => Math.round(a * w + b * (1 - w))
  const toHex = (n: number) => n.toString(16).padStart(2, '0')
  return `#${toHex(blend(ar, br))}${toHex(blend(ag, bg))}${toHex(blend(ab, bb))}`
}

export function applyAccentPalette(
  id: ColorSchemeId,
  theme: 'light' | 'dark' = 'dark',
): void {
  const palette = getAccentPalette(id)
  const root = document.documentElement
  const rowSurface = theme === 'light' ? ROW_SURFACE_LIGHT : ROW_SURFACE_DARK
  const rowActiveBase = theme === 'light' ? ROW_ACTIVE_BASE_LIGHT : ROW_ACTIVE_BASE_DARK
  root.dataset.accent = id
  root.style.setProperty('--app-brand-500', palette.primary)
  root.style.setProperty('--app-brand-600', palette.primaryHover)
  root.style.setProperty('--app-row-hover-bg', mixHex(palette.primary, rowSurface, ROW_HOVER_MIX))
  root.style.setProperty('--app-row-active-bg', mixHex(palette.primary, rowActiveBase, ROW_ACTIVE_MIX))
  root.style.setProperty(
    '--app-progress-fill-dark',
    progressFill(palette.primary, PROGRESS_TRACK_DARK, PROGRESS_MIX_DARK),
  )
  root.style.setProperty(
    '--app-progress-fill-light',
    progressFill(palette.primary, PROGRESS_TRACK_LIGHT, PROGRESS_MIX_LIGHT),
  )
}
