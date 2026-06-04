import { Box, Text } from '@chakra-ui/react'
import { useI18n } from '../../i18n'
import type { MassRenameMode } from '../../features/torrentMassRename'

type MassRenameModeHelpProps = {
  mode: MassRenameMode
}

const MODE_HELP_KEYS: Record<MassRenameMode, string> = {
  findReplace: 'massRename.help.findReplace',
  prefixSuffix: 'massRename.help.prefixSuffix',
  numbering: 'massRename.help.numbering',
  regex: 'massRename.help.regex',
  template: 'massRename.help.template',
}

export function MassRenameModeHelp({ mode }: MassRenameModeHelpProps) {
  const { t, tList } = useI18n()
  const base = MODE_HELP_KEYS[mode]
  const examples = tList(`${base}.examples`)

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
        {t('massRename.general.summary')}
      </Text>
      <Box>
        <Text fontSize="sm" fontWeight="medium" color="fg" mb={1}>
          {t(`${base}.title`)}
        </Text>
        <Text fontSize="xs" color="fg.muted" lineHeight="tall" mb={examples.length > 0 ? 2 : 0}>
          {t(`${base}.summary`)}
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
      <Text fontSize="xs" color="fg.subtle" mt={3} lineHeight="tall">
        {t('massRename.general.stemOnly')} {t('massRename.general.sort')}
      </Text>
    </Box>
  )
}
