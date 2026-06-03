import { Box, Checkbox, Text } from '@chakra-ui/react'
import type { UiSettingsDto } from '../../api/types'

type TraySettingsSectionProps = {
  ui: UiSettingsDto
  onChange: (patch: Partial<UiSettingsDto>) => void
}

export function TraySettingsSection({ ui, onChange }: TraySettingsSectionProps) {
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
        System tray
      </Text>
      <Box display="flex" flexDirection="column" gap={3}>
        <Checkbox.Root
          checked={ui.trayEnabled ?? true}
          onCheckedChange={(e) => onChange({ trayEnabled: e.checked === true })}
        >
          <Checkbox.HiddenInput />
          <Checkbox.Control />
          <Checkbox.Label>Show icon in the system tray</Checkbox.Label>
        </Checkbox.Root>
        <Checkbox.Root
          checked={ui.closeToTray ?? true}
          disabled={!(ui.trayEnabled ?? true)}
          onCheckedChange={(e) => onChange({ closeToTray: e.checked === true })}
        >
          <Checkbox.HiddenInput />
          <Checkbox.Control />
          <Checkbox.Label>Close window to tray instead of quitting</Checkbox.Label>
        </Checkbox.Root>
        <Checkbox.Root
          checked={ui.minimizeToTray ?? false}
          disabled={!(ui.trayEnabled ?? true)}
          onCheckedChange={(e) => onChange({ minimizeToTray: e.checked === true })}
        >
          <Checkbox.HiddenInput />
          <Checkbox.Control />
          <Checkbox.Label>Minimize to tray (when supported by the window)</Checkbox.Label>
        </Checkbox.Root>
      </Box>
    </Box>
  )
}
