import { Box } from '@chakra-ui/react'
import type { ReactNode } from 'react'
import { OVERLAY_Z_INDEX } from '../../lib/overlayRoot'
import { OverlayPortal } from './OverlayPortal'

type ElevatedOverlayProps = {
  children: ReactNode
  insetPadding?: boolean
  onBackdropPointerDown?: (e: React.PointerEvent<HTMLDivElement>) => void
}

export function ElevatedOverlay({
  children,
  insetPadding = true,
  onBackdropPointerDown,
}: ElevatedOverlayProps) {
  return (
    <OverlayPortal>
      <Box
        position="fixed"
        inset={0}
        zIndex={OVERLAY_Z_INDEX.elevated}
        display="flex"
        alignItems="center"
        justifyContent="center"
        p={insetPadding ? { base: 3, md: 6 } : 0}
        bg="blackAlpha.700"
        data-overlay-elevated=""
        onPointerDown={onBackdropPointerDown}
      >
        {children}
      </Box>
    </OverlayPortal>
  )
}
