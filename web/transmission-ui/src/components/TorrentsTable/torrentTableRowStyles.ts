const rowActiveBg = 'var(--app-row-active-bg, #1f1a14)'
const rowHoverBg = 'var(--app-row-hover-bg, #1c1814)'
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
