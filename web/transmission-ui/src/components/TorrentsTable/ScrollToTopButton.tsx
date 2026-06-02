import { IconButton } from '@chakra-ui/react'

function ChevronUpIcon({ size = 22 }: { size?: number }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="currentColor" aria-hidden>
      <path d="M7.41 15.41 12 10.83l4.59 4.58L18 14l-6-6-6 6 1.41 1.41z" />
    </svg>
  )
}

type ScrollToTopButtonProps = {
  visible: boolean
  onClick: () => void
}

export function ScrollToTopButton({ visible, onClick }: ScrollToTopButtonProps) {
  if (!visible) return null

  return (
    <IconButton
      type="button"
      aria-label="Scroll to top"
      title="Back to top"
      position="absolute"
      right={3}
      bottom={3}
      zIndex={3}
      size="lg"
      borderRadius="full"
      variant="outline"
      bg="surface.panel"
      borderColor="brand.500"
      borderWidth="2px"
      color="brand.500"
      boxShadow="0 8px 24px rgba(0, 0, 0, 0.55)"
      _hover={{ bg: 'bg.emphasized', transform: 'translateY(-1px)' }}
      transition="opacity 0.2s, transform 0.15s"
      onClick={onClick}
    >
      <ChevronUpIcon />
    </IconButton>
  )
}
