import { createToaster } from '@chakra-ui/react'

export const appToaster = createToaster({
  placement: 'top-end',
  overlap: true,
  gap: 12,
  max: 5,
})
