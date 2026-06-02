import { Box, type BoxProps } from '@chakra-ui/react'
import { islandPanelStyle } from './islandStyles'

export function IslandPanel(props: BoxProps) {
  return (
    <Box
      display="flex"
      flexDirection="column"
      overflow="hidden"
      minH={0}
      minW={0}
      {...islandPanelStyle}
      {...props}
    />
  )
}
