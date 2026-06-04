import {
  Box,
  Button,
  Field,
  Flex,
  Heading,
  Input,
  SimpleGrid,
  Text,
} from '@chakra-ui/react'
import { useI18n } from '../../i18n'
import type { DaemonConnectionDto } from '../../api/types'
import { buildRpcUrl } from './buildRpcUrl'

type DaemonSettingsSectionProps = {
  daemon: DaemonConnectionDto
  onChange: (daemon: DaemonConnectionDto) => void
  onTest: () => void
  testing?: boolean
}

export function DaemonSettingsSection({
  daemon,
  onChange,
  onTest,
  testing,
}: DaemonSettingsSectionProps) {
  const { t } = useI18n()
  const password = daemon.password ?? ''

  const update = (field: keyof DaemonConnectionDto, value: string | number) => {
    onChange({ ...daemon, [field]: value })
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
          {t('settings.connection.title')}
        </Heading>
        <Text fontSize="sm" color="fg.muted">
          {t('settings.connection.subtitle')}
        </Text>
      </Box>

      <Box px={5} py={5}>
        <Text fontSize="xs" fontWeight="semibold" color="fg.muted" textTransform="uppercase" letterSpacing="wider" mb={3}>
          {t('settings.connection.sectionConnection')}
        </Text>
        <SimpleGrid columns={{ base: 1, md: 2 }} gap={4}>
          <Field.Root>
            <Field.Label>{t('settings.connection.host')}</Field.Label>
            <Input
              value={daemon.host}
              onChange={(e) => update('host', e.target.value)}
              placeholder="127.0.0.1"
              bg="surface.panel"
              borderColor="border"
            />
          </Field.Root>
          <Field.Root>
            <Field.Label>{t('settings.connection.port')}</Field.Label>
            <Input
              type="number"
              min={1}
              max={65535}
              value={daemon.port}
              onChange={(e) => update('port', Number(e.target.value))}
              placeholder="9091"
              bg="surface.panel"
              borderColor="border"
            />
          </Field.Root>
          <Field.Root gridColumn={{ base: '1', md: '1 / -1' }}>
            <Field.Label>{t('settings.connection.rpcPath')}</Field.Label>
            <Input
              value={daemon.rpcPath}
              onChange={(e) => update('rpcPath', e.target.value)}
              placeholder="/transmission/rpc"
              bg="surface.panel"
              borderColor="border"
            />
          </Field.Root>
        </SimpleGrid>

        <Box
          mt={4}
          px={3}
          py={2}
          borderRadius="md"
          borderWidth="1px"
          borderColor="border"
          bg="surface.panel"
        >
          <Text fontSize="xs" color="fg.muted" mb={1}>
            {t('settings.connection.rpcUrl')}
          </Text>
          <Text fontSize="sm" color="fg" fontFamily="mono">
            {buildRpcUrl(daemon)}
          </Text>
        </Box>

        <Text
          fontSize="xs"
          fontWeight="semibold"
          color="fg.muted"
          textTransform="uppercase"
          letterSpacing="wider"
          mt={6}
          mb={3}
        >
          {t('settings.connection.sectionAuth')}
        </Text>
        <SimpleGrid columns={{ base: 1, md: 2 }} gap={4}>
          <Field.Root>
            <Field.Label>{t('settings.connection.username')}</Field.Label>
            <Input
              value={daemon.username}
              onChange={(e) => update('username', e.target.value)}
              autoComplete="username"
              bg="surface.panel"
              borderColor="border"
            />
          </Field.Root>
          <Field.Root>
            <Field.Label>{t('settings.connection.password')}</Field.Label>
            <Input
              type="password"
              value={password}
              onChange={(e) => update('password', e.target.value)}
              placeholder={t('settings.connection.passwordPlaceholder')}
              autoComplete="current-password"
              bg="surface.panel"
              borderColor="border"
            />
            <Field.HelperText color="fg.subtle">
              {t('settings.connection.passwordHint')}
            </Field.HelperText>
          </Field.Root>
        </SimpleGrid>

        <Flex gap={3} mt={6} flexWrap="wrap">
          <Button
            colorPalette="brand"
            variant="outline"
            borderColor="border"
            onClick={onTest}
            loading={testing}
          >
            {t('settings.connection.test')}
          </Button>
        </Flex>
      </Box>
    </Box>
  )
}
