import { Box, Button, IconButton } from '@chakra-ui/react'
import type { ReactNode } from 'react'

type SidebarControlProps = {
  label: string
  onClick: () => void
  children: ReactNode
  showLabel?: boolean
}

const controlHover = {
  bg: 'bg.muted',
  color: 'brand.500',
  borderColor: 'brand.500',
}

export function SidebarControl({ label, onClick, children, showLabel }: SidebarControlProps) {
  if (showLabel) {
    return (
      <Button
        type="button"
        variant="outline"
        size="sm"
        flex="1"
        minW={0}
        borderColor="border"
        bg="bg.emphasized"
        color="fg.muted"
        fontSize="xs"
        gap={1.5}
        _hover={controlHover}
        onClick={onClick}
      >
        {children}
        {label}
      </Button>
    )
  }

  return (
    <IconButton
      type="button"
      aria-label={label}
      title={label}
      variant="outline"
      size="md"
      w="full"
      h="10"
      borderColor="border"
      bg="bg.emphasized"
      color="fg.muted"
      _hover={controlHover}
      onClick={onClick}
    >
      {children}
    </IconButton>
  )
}

export function SidebarControlsGroup({ children }: { children: ReactNode }) {
  return (
    <Box
      mt={4}
      pt={3}
      px={2}
      borderTopWidth="1px"
      borderColor="border"
    >
      <Box display="flex" gap={2} flexDirection="column">
        {children}
      </Box>
    </Box>
  )
}
