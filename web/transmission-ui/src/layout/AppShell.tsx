import { Flex } from '@chakra-ui/react'
import { Outlet } from 'react-router-dom'
import { AppSidebar } from '../components/AppSidebar'
import { IslandPanel } from '../components/layout'
import { StatusBar } from '../components/StatusBar'

const SHELL_GAP = 3

export function AppShell() {
  return (
    <Flex
      h="100vh"
      direction="column"
      bg="surface.canvas"
      overflow="hidden"
      p={SHELL_GAP}
      gap={SHELL_GAP}
    >
      <Flex flex="1" minH={0} minW={0} gap={SHELL_GAP} align="stretch">
        <AppSidebar />

        <IslandPanel flex="1" minH={0} minW={0} p={4}>
          <Outlet />
        </IslandPanel>
      </Flex>

      <StatusBar />
    </Flex>
  )
}
