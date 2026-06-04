import type { NavItemConfig } from './types'

export const NAV_ITEMS: NavItemConfig[] = [
  { to: '/', labelKey: 'nav.torrents', end: true },
  { to: '/add', labelKey: 'nav.addTorrent' },
  { to: '/settings', labelKey: 'nav.settings' },
]
