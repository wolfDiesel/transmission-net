import {
  Box,
  Button,
  Checkbox,
  Field,
  Flex,
  Heading,
  Input,
  SimpleGrid,
  Spinner,
  Text,
} from '@chakra-ui/react'
import { useI18n } from '../../i18n'
import type { DaemonSessionSettingsDto } from '../../api/types'

type DaemonSessionSettingsSectionProps = {
  settings: DaemonSessionSettingsDto | null
  loading: boolean
  loadError: string | null
  saving: boolean
  onChange: (settings: DaemonSessionSettingsDto) => void
  onReload: () => void
  onSave: () => void
}

function BoolField({
  label,
  checked,
  onCheckedChange,
}: {
  label: string
  checked: boolean
  onCheckedChange: (checked: boolean) => void
}) {
  return (
    <Checkbox.Root
      checked={checked}
      onCheckedChange={(e) => onCheckedChange(Boolean(e.checked))}
    >
      <Checkbox.HiddenInput />
      <Checkbox.Control borderColor="border" />
      <Checkbox.Label color="fg" fontSize="sm">
        {label}
      </Checkbox.Label>
    </Checkbox.Root>
  )
}

export function DaemonSessionSettingsSection({
  settings,
  loading,
  loadError,
  saving,
  onChange,
  onReload,
  onSave,
}: DaemonSessionSettingsSectionProps) {
  const { t } = useI18n()

  if (!settings && loading) {
    return (
      <Flex justify="center" py={8}>
        <Spinner color="brand.500" />
      </Flex>
    )
  }

  if (!settings) {
    return (
      <Box
        borderWidth="1px"
        borderColor="border"
        borderRadius="md"
        bg="bg.emphasized"
        px={5}
        py={5}
      >
        <Text fontSize="sm" color="fg.muted" mb={3}>
          {loadError ?? t('settings.daemon.loadErrorFallback')}
        </Text>
        <Button variant="outline" borderColor="border" onClick={onReload}>
          {t('settings.daemon.retry')}
        </Button>
      </Box>
    )
  }

  const update = <K extends keyof DaemonSessionSettingsDto>(
    field: K,
    value: DaemonSessionSettingsDto[K],
  ) => {
    onChange({ ...settings, [field]: value })
  }

  return (
    <Box
      borderWidth="1px"
      borderColor="border"
      borderRadius="md"
      bg="bg.emphasized"
      overflow="hidden"
    >
      <Box px={5} py={4} borderBottomWidth="1px" borderColor="border" bg="surface.panel">
        <Heading size="sm" color="brand.500" mb={1}>
          {t('settings.daemon.preferencesTitle')}
        </Heading>
        <Text fontSize="sm" color="fg.muted">
          {t('settings.daemon.preferencesSubtitle')}
        </Text>
      </Box>

      <Box px={5} py={5} opacity={loading ? 0.6 : 1}>
        <Text
          fontSize="xs"
          fontWeight="semibold"
          color="fg.muted"
          textTransform="uppercase"
          letterSpacing="wider"
          mb={3}
        >
          {t('settings.daemon.storage')}
        </Text>
        <SimpleGrid columns={{ base: 1, md: 2 }} gap={4}>
          <Field.Root gridColumn={{ base: '1', md: '1 / -1' }}>
            <Field.Label>{t('settings.daemon.downloadDir')}</Field.Label>
            <Input
              value={settings.downloadDir}
              onChange={(e) => update('downloadDir', e.target.value)}
              bg="surface.panel"
              borderColor="border"
            />
          </Field.Root>
          <Field.Root gridColumn={{ base: '1', md: '1 / -1' }}>
            <Field.Label>{t('settings.daemon.incompleteDir')}</Field.Label>
            <Input
              value={settings.incompleteDir}
              onChange={(e) => update('incompleteDir', e.target.value)}
              bg="surface.panel"
              borderColor="border"
            />
          </Field.Root>
        </SimpleGrid>
        <Flex direction="column" gap={2} mt={4}>
          <BoolField
            label={t('settings.daemon.incompleteDirEnabled')}
            checked={settings.incompleteDirEnabled}
            onCheckedChange={(v) => update('incompleteDirEnabled', v)}
          />
          <BoolField
            label={t('settings.daemon.trashTorrent')}
            checked={settings.trashOriginalTorrentFiles}
            onCheckedChange={(v) => update('trashOriginalTorrentFiles', v)}
          />
        </Flex>

        <Text
          fontSize="xs"
          fontWeight="semibold"
          color="fg.muted"
          textTransform="uppercase"
          letterSpacing="wider"
          mt={6}
          mb={3}
        >
          {t('settings.daemon.peers')}
        </Text>
        <SimpleGrid columns={{ base: 1, md: 2 }} gap={4}>
          <Field.Root>
            <Field.Label>{t('settings.daemon.globalPeerLimit')}</Field.Label>
            <Input
              type="number"
              min={0}
              value={settings.peerLimitGlobal}
              onChange={(e) => update('peerLimitGlobal', Number(e.target.value))}
              bg="surface.panel"
              borderColor="border"
            />
          </Field.Root>
          <Field.Root>
            <Field.Label>{t('settings.daemon.peerLimitPerTorrent')}</Field.Label>
            <Input
              type="number"
              min={0}
              value={settings.peerLimitPerTorrent}
              onChange={(e) => update('peerLimitPerTorrent', Number(e.target.value))}
              bg="surface.panel"
              borderColor="border"
            />
          </Field.Root>
        </SimpleGrid>

        <Text
          fontSize="xs"
          fontWeight="semibold"
          color="fg.muted"
          textTransform="uppercase"
          letterSpacing="wider"
          mt={6}
          mb={3}
        >
          {t('settings.daemon.speedLimits')}
        </Text>
        <SimpleGrid columns={{ base: 1, md: 2 }} gap={4}>
          <Field.Root>
            <Field.Label>{t('settings.daemon.downloadLimit')}</Field.Label>
            <Input
              type="number"
              min={0}
              value={settings.speedLimitDownKbps}
              onChange={(e) => update('speedLimitDownKbps', Number(e.target.value))}
              disabled={!settings.speedLimitDownEnabled}
              bg="surface.panel"
              borderColor="border"
            />
          </Field.Root>
          <Field.Root>
            <Field.Label>{t('settings.daemon.uploadLimit')}</Field.Label>
            <Input
              type="number"
              min={0}
              value={settings.speedLimitUpKbps}
              onChange={(e) => update('speedLimitUpKbps', Number(e.target.value))}
              disabled={!settings.speedLimitUpEnabled}
              bg="surface.panel"
              borderColor="border"
            />
          </Field.Root>
        </SimpleGrid>
        <Flex direction="column" gap={2} mt={4}>
          <BoolField
            label={t('settings.daemon.enableDownloadLimit')}
            checked={settings.speedLimitDownEnabled}
            onCheckedChange={(v) => update('speedLimitDownEnabled', v)}
          />
          <BoolField
            label={t('settings.daemon.enableUploadLimit')}
            checked={settings.speedLimitUpEnabled}
            onCheckedChange={(v) => update('speedLimitUpEnabled', v)}
          />
        </Flex>

        <Text
          fontSize="xs"
          fontWeight="semibold"
          color="fg.muted"
          textTransform="uppercase"
          letterSpacing="wider"
          mt={6}
          mb={3}
        >
          {t('settings.daemon.seeding')}
        </Text>
        <SimpleGrid columns={{ base: 1, md: 2 }} gap={4}>
          <Field.Root>
            <Field.Label>{t('settings.daemon.seedRatioLimit')}</Field.Label>
            <Input
              type="number"
              min={0}
              step={0.1}
              value={settings.seedRatioLimit}
              onChange={(e) => update('seedRatioLimit', Number(e.target.value))}
              disabled={!settings.seedRatioLimited}
              bg="surface.panel"
              borderColor="border"
            />
          </Field.Root>
          <Field.Root>
            <Field.Label>{t('settings.daemon.idleSeedingLimit')}</Field.Label>
            <Input
              type="number"
              min={0}
              value={settings.idleSeedingLimitMinutes}
              onChange={(e) => update('idleSeedingLimitMinutes', Number(e.target.value))}
              disabled={!settings.idleSeedingLimitEnabled}
              bg="surface.panel"
              borderColor="border"
            />
          </Field.Root>
        </SimpleGrid>
        <Flex direction="column" gap={2} mt={4}>
          <BoolField
            label={t('settings.daemon.seedRatioLimited')}
            checked={settings.seedRatioLimited}
            onCheckedChange={(v) => update('seedRatioLimited', v)}
          />
          <BoolField
            label={t('settings.daemon.idleSeedingLimitEnabled')}
            checked={settings.idleSeedingLimitEnabled}
            onCheckedChange={(v) => update('idleSeedingLimitEnabled', v)}
          />
        </Flex>

        <Flex gap={3} mt={6} flexWrap="wrap">
          <Button
            colorPalette="brand"
            onClick={onSave}
            loading={saving}
          >
            {t('settings.daemon.apply')}
          </Button>
          <Button
            variant="outline"
            borderColor="border"
            onClick={onReload}
            loading={loading}
          >
            {t('settings.daemon.reload')}
          </Button>
        </Flex>
      </Box>
    </Box>
  )
}
