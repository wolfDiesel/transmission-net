import { Box, Checkbox, Text } from '@chakra-ui/react'
import { useI18n } from '../../i18n'
import type { UiSettingsDto } from '../../api/types'

type TraySettingsSectionProps = {
  ui: UiSettingsDto
  onChange: (patch: Partial<UiSettingsDto>) => void
}

export function TraySettingsSection({ ui, onChange }: TraySettingsSectionProps) {
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
        {t('settings.tray.title')}
      </Text>
      <Box display="flex" flexDirection="column" gap={3}>
        <Checkbox.Root
          checked={ui.trayEnabled ?? true}
          onCheckedChange={(e) => onChange({ trayEnabled: e.checked === true })}
        >
          <Checkbox.HiddenInput />
          <Checkbox.Control />
          <Checkbox.Label>{t('settings.tray.enabled')}</Checkbox.Label>
        </Checkbox.Root>
        <Checkbox.Root
          checked={ui.closeToTray ?? true}
          disabled={!(ui.trayEnabled ?? true)}
          onCheckedChange={(e) => onChange({ closeToTray: e.checked === true })}
        >
          <Checkbox.HiddenInput />
          <Checkbox.Control />
          <Checkbox.Label>{t('settings.tray.closeToTray')}</Checkbox.Label>
        </Checkbox.Root>
        <Checkbox.Root
          checked={ui.minimizeToTray ?? false}
          disabled={!(ui.trayEnabled ?? true)}
          onCheckedChange={(e) => onChange({ minimizeToTray: e.checked === true })}
        >
          <Checkbox.HiddenInput />
          <Checkbox.Control />
          <Checkbox.Label>{t('settings.tray.minimizeToTray')}</Checkbox.Label>
        </Checkbox.Root>
      </Box>
    </Box>
  )
}
