import { Box, Text } from '@chakra-ui/react'
import {
  MASS_RENAME_GENERAL_HELP,
  MASS_RENAME_MODE_HELP,
  type MassRenameModeHelp,
} from '../../features/torrentMassRename/modeHelp'
import type { MassRenameMode } from '../../features/torrentMassRename'

type MassRenameModeHelpProps = {
  mode: MassRenameMode
}

function HelpBlock({ title, summary, examples }: MassRenameModeHelp) {
  return (
    <Box>
      <Text fontSize="sm" fontWeight="medium" color="fg" mb={1}>
        {title}
      </Text>
      <Text fontSize="xs" color="fg.muted" lineHeight="tall" mb={examples.length > 0 ? 2 : 0}>
        {summary}
      </Text>
      {examples.length > 0 && (
        <Box as="ul" m={0} pl={4} fontSize="xs" color="fg.subtle" lineHeight="tall">
          {examples.map((example) => (
            <Box as="li" key={example} mb={0.5}>
              {example}
            </Box>
          ))}
        </Box>
      )}
    </Box>
  )
}

export function MassRenameModeHelp({ mode }: MassRenameModeHelpProps) {
  const modeHelp = MASS_RENAME_MODE_HELP[mode]

  return (
    <Box
      mb={4}
      px={3}
      py={3}
      borderWidth="1px"
      borderColor="border"
      borderRadius="md"
      bg="bg.emphasized"
    >
      <Text fontSize="xs" color="fg.muted" lineHeight="tall" mb={3}>
        {MASS_RENAME_GENERAL_HELP.summary}
      </Text>
      <HelpBlock {...modeHelp} />
      <Text fontSize="xs" color="fg.subtle" mt={3} lineHeight="tall">
        {MASS_RENAME_GENERAL_HELP.stemOnly} {MASS_RENAME_GENERAL_HELP.sort}
      </Text>
    </Box>
  )
}
