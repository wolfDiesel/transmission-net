import { Flex, Image, Text } from '@chakra-ui/react'

type AppLogoProps = {
  size?: number
  showLabel?: boolean
  labelSize?: 'sm' | 'md'
}

export function AppLogo({ size = 36, showLabel = false, labelSize = 'sm' }: AppLogoProps) {
  return (
    <Flex align="center" gap={showLabel ? 3 : 0} minW={0} title="TransmissionNET">
      <Image
        src="/transmission-net.svg"
        alt=""
        w={`${size}px`}
        h={`${size}px`}
        flexShrink={0}
        draggable={false}
      />
      {showLabel && (
        <Text
          fontSize={labelSize}
          fontWeight="bold"
          color="brand.500"
          letterSpacing="wider"
          whiteSpace="nowrap"
        >
          TransmissionNET
        </Text>
      )}
    </Flex>
  )
}

export function AppLogoMark({ size = 28 }: { size?: number }) {
  return (
    <Image
      src="/transmission-net.svg"
      alt=""
      w={`${size}px`}
      h={`${size}px`}
      flexShrink={0}
      draggable={false}
      title="TransmissionNET"
    />
  )
}
