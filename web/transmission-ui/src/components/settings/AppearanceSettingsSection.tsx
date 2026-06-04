import {
  Box,
  Button,
  Flex,
  RadioGroup,
  SimpleGrid,
  Text,
} from '@chakra-ui/react'
import { useI18n } from '../../i18n'
import type { AppearanceId, ColorSchemeId } from '../../theme/accentPalettes'
import { ACCENT_PALETTES, APPEARANCE_IDS } from '../../theme/accentPalettes'

const APPEARANCE_VALUES: AppearanceId[] = ['light', 'dark', 'system']

type AppearanceSettingsSectionProps = {
  colorScheme: ColorSchemeId
  appearance: AppearanceId
  onColorSchemeChange: (value: ColorSchemeId) => void
  onAppearanceChange: (value: AppearanceId) => void
}

export function AppearanceSettingsSection({
  colorScheme,
  appearance,
  onColorSchemeChange,
  onAppearanceChange,
}: AppearanceSettingsSectionProps) {
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
        {t('settings.appearance.title')}
      </Text>

      <Text fontSize="xs" fontWeight="semibold" color="fg.muted" textTransform="uppercase" letterSpacing="wider" mb={3}>
        {t('settings.appearance.theme')}
      </Text>
      <RadioGroup.Root
        value={appearance}
        onValueChange={(e) => {
          const next = e.value as AppearanceId
          if (APPEARANCE_IDS.includes(next)) onAppearanceChange(next)
        }}
        colorPalette="brand"
      >
        <Flex gap={3} flexWrap="wrap">
          {APPEARANCE_VALUES.map((value) => (
            <RadioGroup.Item key={value} value={value}>
              <RadioGroup.ItemHiddenInput />
              <RadioGroup.ItemIndicator />
              <RadioGroup.ItemText color="fg">{t(`settings.appearance.${value}`)}</RadioGroup.ItemText>
            </RadioGroup.Item>
          ))}
        </Flex>
      </RadioGroup.Root>

      <Text
        fontSize="xs"
        fontWeight="semibold"
        color="fg.muted"
        textTransform="uppercase"
        letterSpacing="wider"
        mt={6}
        mb={3}
      >
        {t('settings.appearance.colorScheme')}
      </Text>
      <SimpleGrid columns={{ base: 2, sm: 3, md: 5 }} gap={3}>
        {ACCENT_PALETTES.map((palette) => {
          const selected = colorScheme === palette.id
          return (
            <Button
              key={palette.id}
              variant="outline"
              onClick={() => onColorSchemeChange(palette.id)}
              borderWidth="2px"
              borderColor={selected ? 'brand.500' : 'border'}
              borderRadius="md"
              bg="surface.panel"
              h="auto"
              px={3}
              py={3}
              justifyContent="flex-start"
              fontWeight="normal"
              _hover={{ borderColor: selected ? 'brand.500' : 'border.emphasized' }}
            >
              <Flex align="center" gap={2}>
                <Box
                  w="18px"
                  h="18px"
                  borderRadius="full"
                  bg={palette.primary}
                  borderWidth="1px"
                  borderColor="border"
                  flexShrink={0}
                />
                <Text fontSize="sm" color="fg" fontWeight={selected ? 'semibold' : 'normal'}>
                  {t(`settings.appearance.colors.${palette.id}`)}
                </Text>
              </Flex>
            </Button>
          )
        })}
      </SimpleGrid>
    </Box>
  )
}
