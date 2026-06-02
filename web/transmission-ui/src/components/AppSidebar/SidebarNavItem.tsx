import { Box } from '@chakra-ui/react'
import { NavLink } from 'react-router-dom'
import { navIconForPath } from './SidebarIcons'
import type { NavItemConfig } from './types'

type SidebarNavItemProps = {
  item: NavItemConfig
  compact?: boolean
  onNavigate?: () => void
}

const activeStyles = {
  color: 'brand.500',
  bg: 'bg.emphasized',
  borderColor: 'brand.500',
  fontWeight: 'semibold',
}

const idleStyles = {
  color: 'fg.muted',
  bg: 'transparent',
  borderColor: 'transparent',
  fontWeight: 'normal',
}

export function SidebarNavItem({ item, compact, onNavigate }: SidebarNavItemProps) {
  return (
    <NavLink to={item.to} end={item.end} style={{ textDecoration: 'none' }} onClick={onNavigate}>
      {({ isActive }) =>
        compact ? (
          <Box
            display="flex"
            alignItems="center"
            justifyContent="center"
            w="full"
            h="11"
            borderRadius="md"
            borderWidth="1px"
            title={item.label}
            aria-label={item.label}
            transition="all 0.15s"
            {...(isActive ? activeStyles : idleStyles)}
            _hover={{
              color: 'brand.500',
              borderColor: 'brand.500',
              bg: 'bg.emphasized',
            }}
          >
            {navIconForPath(item.to, 22)}
          </Box>
        ) : (
          <Box
            display="flex"
            alignItems="center"
            gap={3}
            mx={2}
            px={3}
            py={2.5}
            mb={1}
            borderRadius="md"
            borderWidth="1px"
            borderLeftWidth="3px"
            borderLeftColor={isActive ? 'brand.500' : 'transparent'}
            transition="all 0.15s"
            {...(isActive ? activeStyles : idleStyles)}
            _hover={{
              color: 'brand.500',
              borderColor: 'brand.500',
              bg: 'bg.emphasized',
            }}
          >
            {navIconForPath(item.to, 20)}
            {item.label}
          </Box>
        )
      }
    </NavLink>
  )
}
