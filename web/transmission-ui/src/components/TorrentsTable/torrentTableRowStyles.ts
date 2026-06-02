const rowActiveBg =
  'color-mix(in srgb, var(--app-brand-500, #F07818) 11%, var(--chakra-colors-bg-emphasized, #141414))'
const rowHoverBg =
  'color-mix(in srgb, var(--app-brand-500, #F07818) 7%, var(--chakra-colors-surface-raised, #1A1A1A))'
const rowMenuBar = 'inset 4px 0 0 var(--app-brand-500, #F07818)'

export function getTorrentTableRowProps(
  torrentId: number,
  selectedId: number | null,
  menuRowId: number | null,
) {
  const isSelected = selectedId === torrentId
  const isMenuActive = menuRowId === torrentId
  const isActive = isSelected || isMenuActive

  return {
    cursor: 'pointer' as const,
    bg: isActive ? rowActiveBg : undefined,
    boxShadow: isMenuActive ? rowMenuBar : undefined,
    transition: 'background-color 0.12s, box-shadow 0.12s',
    _hover: {
      bg: isActive ? rowActiveBg : rowHoverBg,
      boxShadow: isMenuActive ? rowMenuBar : undefined,
    },
  }
}
