import { Flex, Text } from '@chakra-ui/react'
import type { ReactNode } from 'react'

type PropertyRowProps = {
  label: string
  value: ReactNode
}

export function PropertyRow({ label, value }: PropertyRowProps) {
  return (
    <Flex
      py={2}
      gap={4}
      borderBottomWidth="1px"
      borderColor="border.muted"
      align="flex-start"
    >
      <Text w="36%" flexShrink={0} fontSize="sm" color="fg.muted">
        {label}
      </Text>
      <Text flex="1" fontSize="sm" color="fg" wordBreak="break-word">
        {value}
      </Text>
    </Flex>
  )
}
