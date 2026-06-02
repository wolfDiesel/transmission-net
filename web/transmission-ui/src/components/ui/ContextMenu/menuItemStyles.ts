const menuItemHighlightBg =
  'color-mix(in srgb, var(--app-brand-500, #F07818) 14%, var(--chakra-colors-surface-raised, #1A1A1A))'

export const contextMenuItemProps = {
  borderRadius: 'sm',
  color: 'fg',
  minH: '32px',
  transition: 'background-color 0.12s, color 0.12s',
  _highlighted: {
    bg: menuItemHighlightBg,
    color: 'brand.500',
  },
} as const

export const contextMenuDangerItemProps = {
  ...contextMenuItemProps,
  color: 'red.400',
  _highlighted: {
    bg: 'rgba(248, 113, 113, 0.14)',
    color: 'red.300',
  },
} as const

export const contextMenuContentProps = {
  bg: 'bg.emphasized',
  borderColor: 'border',
  borderWidth: '1px',
  borderRadius: 'md',
  py: 1,
  boxShadow: 'lg',
} as const
