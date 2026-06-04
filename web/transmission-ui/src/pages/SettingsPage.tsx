import {
  Box,
  Button,
  Field,
  Flex,
  Input,
  SimpleGrid,
  Spinner,
  Tabs,
  Text,
} from '@chakra-ui/react'
import { useEffect, useState } from 'react'
import { api, ApiError } from '../api/client'
import {
  AppearanceSettingsSection,
  DaemonSessionSettingsSection,
  DaemonSettingsSection,
  LanguageSettingsSection,
  TorrentFileAssociationSection,
  TraySettingsSection,
} from '../components/settings'
import { useI18n } from '../i18n'
import type { LocaleCode } from '../i18n'
import { normalizeAppearance, normalizeColorScheme } from '../theme/accentPalettes'
import type { DaemonSessionSettingsDto, DesktopCapabilitiesDto } from '../api/types'
import { showAppToast } from '../components/AppToast'
import { useApp } from '../context/AppProvider'

export function SettingsPage() {
  const { t } = useI18n()
  const {
    settings,
    setSettings,
    settingsLoading,
    settingsError,
    applySavedSettings,
  } = useApp()
  const [busy, setBusy] = useState<'test' | 'save' | 'daemon-save' | null>(null)
  const [tab, setTab] = useState('connection')
  const [daemonSession, setDaemonSession] = useState<DaemonSessionSettingsDto | null>(null)
  const [daemonSessionLoading, setDaemonSessionLoading] = useState(false)
  const [daemonSessionError, setDaemonSessionError] = useState<string | null>(null)
  const [desktopCaps, setDesktopCaps] = useState<DesktopCapabilitiesDto | null>(null)

  useEffect(() => {
    void api.getDesktopCapabilities().then(setDesktopCaps).catch(() => setDesktopCaps(null))
  }, [])

  const loadDaemonSession = async () => {
    setDaemonSessionLoading(true)
    setDaemonSessionError(null)
    try {
      const loaded = await api.getDaemonSessionSettings()
      setDaemonSession(loaded)
    } catch (e) {
      setDaemonSession(null)
      setDaemonSessionError(e instanceof ApiError ? e.message : 'Failed to load daemon settings')
    } finally {
      setDaemonSessionLoading(false)
    }
  }

  useEffect(() => {
    if (tab === 'daemon' && !settingsLoading) {
      void loadDaemonSession()
    }
  }, [tab, settingsLoading])

  const showAppSave = tab === 'connection' || tab === 'ui'

  useEffect(() => {
    if (settingsError) {
      showAppToast({
        title: settingsError === 'load_failed' ? t('settings.loadFailed') : settingsError,
        variant: 'error',
      })
    }
  }, [settingsError, t])

  if (settingsLoading) {
    return (
      <Flex justify="center" align="center" flex="1">
        <Spinner color="brand.500" />
      </Flex>
    )
  }

  const updateUi = (field: keyof typeof settings.ui, value: number) => {
    setSettings((prev) => ({
      ...prev,
      ui: { ...prev.ui, [field]: value },
    }))
  }

  const handleTest = async () => {
    setBusy('test')
    try {
      await api.testConnection({
        ...settings.daemon,
        password: settings.daemon.password || null,
      })
      showAppToast({ title: t('settings.connection.connectionOk'), variant: 'success' })
    } catch (e) {
      showAppToast({
        title: e instanceof ApiError ? e.message : t('settings.connection.connectionFailed'),
        variant: 'error',
      })
    } finally {
      setBusy(null)
    }
  }

  const handleSaveDaemonSession = async () => {
    if (!daemonSession) return
    setBusy('daemon-save')
    try {
      const saved = await api.saveDaemonSessionSettings(daemonSession)
      setDaemonSession(saved)
      showAppToast({ title: t('settings.daemon.applied'), variant: 'success' })
    } catch (e) {
      showAppToast({
        title: e instanceof ApiError ? e.message : t('settings.daemon.saveFailed'),
        variant: 'error',
      })
    } finally {
      setBusy(null)
    }
  }

  const handleSave = async () => {
    setBusy('save')
    const password = settings.daemon.password ?? ''
    try {
      const saved = await api.saveSettings({
        ...settings,
        daemon: {
          ...settings.daemon,
          password: password || null,
        },
      })
      applySavedSettings(saved, password)
      showAppToast({ title: t('settings.saved'), variant: 'success' })
    } catch (e) {
      showAppToast({
        title: e instanceof ApiError ? e.message : t('settings.saveFailed'),
        variant: 'error',
      })
    } finally {
      setBusy(null)
    }
  }

  return (
    <Box display="flex" flexDirection="column" flex="1" minH={0} maxW="800px" w="full" overflow="hidden" gap={4}>
      <Box flexShrink={0}>
        <Text fontSize="xl" fontWeight="bold" color="fg">
          {t('settings.title')}
        </Text>
        <Text fontSize="sm" color="fg.muted">
          {t('settings.subtitle')}
        </Text>
      </Box>

      <Tabs.Root
        value={tab}
        onValueChange={(e) => setTab(e.value)}
        variant="line"
        colorPalette="brand"
        size="sm"
        flex="1"
        minH={0}
        display="flex"
        flexDirection="column"
      >
        <Tabs.List borderColor="border" flexShrink={0}>
          <Tabs.Trigger value="connection">{t('settings.tabs.connection')}</Tabs.Trigger>
          <Tabs.Trigger value="daemon">{t('settings.tabs.daemon')}</Tabs.Trigger>
          <Tabs.Trigger value="ui">{t('settings.tabs.ui')}</Tabs.Trigger>
        </Tabs.List>

        <Tabs.ContentGroup flex="1" minH={0} overflow="hidden" display="flex" flexDirection="column">
        <Tabs.Content
          value="connection"
          pt={4}
          flex="1"
          minH={0}
          display="flex"
          flexDirection="column"
          overflow="hidden"
        >
          <Box flex="1" minH={0} overflowY="auto" pr={1}>
            <DaemonSettingsSection
              daemon={{
                ...settings.daemon,
                password: settings.daemon.password ?? '',
              }}
              onChange={(daemon) =>
                setSettings((prev) => ({
                  ...prev,
                  daemon: { ...daemon, password: daemon.password ?? '' },
                }))
              }
              onTest={() => void handleTest()}
              testing={busy === 'test'}
            />
          </Box>
        </Tabs.Content>

        <Tabs.Content
          value="daemon"
          pt={4}
          flex="1"
          minH={0}
          display="flex"
          flexDirection="column"
          overflow="hidden"
        >
          <Box flex="1" minH={0} overflowY="auto" pr={1}>
            <DaemonSessionSettingsSection
              settings={daemonSession}
              loading={daemonSessionLoading}
              loadError={daemonSessionError}
              saving={busy === 'daemon-save'}
              onChange={setDaemonSession}
              onReload={() => void loadDaemonSession()}
              onSave={() => void handleSaveDaemonSession()}
            />
          </Box>
        </Tabs.Content>

        <Tabs.Content
          value="ui"
          pt={4}
          flex="1"
          minH={0}
          display="flex"
          flexDirection="column"
          overflow="hidden"
        >
          <Box flex="1" minH={0} overflowY="auto" pr={1} display="flex" flexDirection="column" gap={4}>
            <LanguageSettingsSection
              language={settings.ui.language ?? 'en'}
              onChange={(language: LocaleCode) =>
                setSettings((prev) => ({
                  ...prev,
                  ui: { ...prev.ui, language },
                }))
              }
            />
            <AppearanceSettingsSection
              colorScheme={normalizeColorScheme(settings.ui.colorScheme)}
              appearance={normalizeAppearance(settings.ui.appearance)}
              onColorSchemeChange={(colorScheme) =>
                setSettings((prev) => ({
                  ...prev,
                  ui: { ...prev.ui, colorScheme },
                }))
              }
              onAppearanceChange={(appearance) =>
                setSettings((prev) => ({
                  ...prev,
                  ui: { ...prev.ui, appearance },
                }))
              }
            />
            <TorrentFileAssociationSection
              onRegistered={() =>
                setSettings((prev) => ({
                  ...prev,
                  ui: { ...prev.ui, torrentFileAssociation: 'registered' },
                }))
              }
            />
            {desktopCaps?.traySettingsAvailable ? (
              <TraySettingsSection
                ui={settings.ui}
                onChange={(patch) =>
                  setSettings((prev) => ({
                    ...prev,
                    ui: { ...prev.ui, ...patch },
                  }))
                }
              />
            ) : null}
          <Box
            borderWidth="1px"
            borderColor="border"
            borderRadius="md"
            bg="bg.emphasized"
            px={5}
            py={5}
          >
            <Text fontSize="sm" fontWeight="semibold" color="brand.500" mb={4}>
              {t('settings.window.title')}
            </Text>
            <SimpleGrid columns={{ base: 1, md: 2 }} gap={4}>
              <Field.Root>
                <Field.Label>{t('settings.window.refreshInterval')}</Field.Label>
                <Input
                  type="number"
                  min={1}
                  value={settings.ui.refreshIntervalSeconds}
                  onChange={(e) => updateUi('refreshIntervalSeconds', Number(e.target.value))}
                  bg="surface.panel"
                  borderColor="border"
                />
              </Field.Root>
              <Field.Root>
                <Field.Label>{t('settings.window.windowWidth')}</Field.Label>
                <Input
                  type="number"
                  min={320}
                  value={settings.ui.windowWidth}
                  onChange={(e) => updateUi('windowWidth', Number(e.target.value))}
                  bg="surface.panel"
                  borderColor="border"
                />
              </Field.Root>
              <Field.Root>
                <Field.Label>{t('settings.window.windowHeight')}</Field.Label>
                <Input
                  type="number"
                  min={240}
                  value={settings.ui.windowHeight}
                  onChange={(e) => updateUi('windowHeight', Number(e.target.value))}
                  bg="surface.panel"
                  borderColor="border"
                />
              </Field.Root>
            </SimpleGrid>
            <Text fontSize="sm" color="fg.muted" mt={3}>
              {t('settings.window.hint')}
            </Text>
          </Box>
          </Box>
        </Tabs.Content>
        </Tabs.ContentGroup>
      </Tabs.Root>

      {showAppSave && (
        <Flex flexShrink={0} pt={2}>
          <Button colorPalette="brand" onClick={() => void handleSave()} loading={busy === 'save'}>
            {t('common.saveSettings')}
          </Button>
        </Flex>
      )}
    </Box>
  )
}
