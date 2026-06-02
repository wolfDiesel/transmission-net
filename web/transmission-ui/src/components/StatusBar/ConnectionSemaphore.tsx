import { Box } from '@chakra-ui/react'

type ConnectionSemaphoreProps = {
  connected: boolean
}

export function ConnectionSemaphore({ connected }: ConnectionSemaphoreProps) {
  const color = connected ? '#22c55e' : '#ef4444'
  const label = connected ? 'Daemon online' : 'Daemon offline'

  return (
    <Box
      as="span"
      display="inline-flex"
      alignItems="center"
      title={label}
      aria-label={label}
      role="status"
    >
      <Box
        w="12px"
        h="12px"
        borderRadius="full"
        bg={color}
        boxShadow={`0 0 8px ${color}`}
        borderWidth="1px"
        borderColor="whiteAlpha.400"
      />
    </Box>
  )
}
