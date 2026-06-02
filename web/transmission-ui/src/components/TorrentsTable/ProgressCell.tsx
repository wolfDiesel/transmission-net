import { Progress } from '@chakra-ui/react'
import { formatPercent } from '../../utils/format'

type ProgressCellProps = {
  percentDone: number
}

export function ProgressCell({ percentDone }: ProgressCellProps) {
  const value = Math.round(Math.min(Math.max(percentDone, 0), 1) * 100)

  return (
    <Progress.Root value={value} min={0} max={100} variant="outline" shape="rounded" w="100%" minW="100px">
      <Progress.Track
        h="20px"
        borderRadius="md"
        bg="progress.track"
        borderWidth="1px"
        borderColor="border"
        overflow="hidden"
        position="relative"
      >
        <Progress.Range h="full" bg="progress.fill" borderRadius="md" />
        <Progress.ValueText
          position="absolute"
          inset={0}
          display="flex"
          alignItems="center"
          justifyContent="center"
          fontSize="xs"
          fontWeight="semibold"
          color="fg"
          lineHeight="1"
          zIndex={1}
        >
          {formatPercent(percentDone)}
        </Progress.ValueText>
      </Progress.Track>
    </Progress.Root>
  )
}
