import { Box, Field, NativeSelect, Text } from '@chakra-ui/react'
import { useI18n } from '../../i18n'
import type { LocaleCode } from '../../i18n'

type LanguageSettingsSectionProps = {
  language: string
  onChange: (language: LocaleCode) => void
}

export function LanguageSettingsSection({ language, onChange }: LanguageSettingsSectionProps) {
  const { t } = useI18n()

  return (
    <Box
      borderWidth="1px"
      borderColor="border"
      borderRadius="md"
      bg="bg.emphasized"
      px={5}
      py={5}
    >
      <Text fontSize="sm" fontWeight="semibold" color="brand.500" mb={4}>
        {t('settings.language.title')}
      </Text>
      <Field.Root maxW="xs">
        <Field.Label>{t('settings.language.label')}</Field.Label>
        <NativeSelect.Root>
          <NativeSelect.Field
            value={language === 'ru' ? 'ru' : 'en'}
            bg="surface.panel"
            borderColor="border"
            onChange={(e) => onChange(e.currentTarget.value === 'ru' ? 'ru' : 'en')}
          >
            <option value="en">{t('settings.language.en')}</option>
            <option value="ru">{t('settings.language.ru')}</option>
          </NativeSelect.Field>
        </NativeSelect.Root>
      </Field.Root>
    </Box>
  )
}
