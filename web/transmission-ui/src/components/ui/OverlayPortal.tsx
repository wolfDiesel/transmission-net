import { Portal } from '@chakra-ui/react'
import { useRef, type ReactNode } from 'react'
import { getOverlayRoot } from '../../lib/overlayRoot'

type OverlayPortalProps = {
  children: ReactNode
}

export function OverlayPortal({ children }: OverlayPortalProps) {
  const containerRef = useRef<HTMLElement | null>(null)
  if (containerRef.current === null) {
    containerRef.current = getOverlayRoot()
  }

  return <Portal container={containerRef}>{children}</Portal>
}
