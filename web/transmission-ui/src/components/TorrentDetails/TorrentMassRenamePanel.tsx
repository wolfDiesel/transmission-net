import {
  Box,
  Button,
  Checkbox,
  Field,
  Flex,
  Input,
  Tabs,
  Text,
} from '@chakra-ui/react'
import { useEffect, useMemo, useState, type ChangeEvent } from 'react'
import { useDebouncedValue } from '../../hooks/useDebouncedValue'
import type { TorrentFileNodeDto } from '../../api/types'
import {
  buildRenamePlan,
  collectScopeFiles,
  defaultMassRenameRule,
  formatScopeLabel,
  validateMassRenameRule,
  validatePlan,
  type MassRenameMode,
  type MassRenameRule,
} from '../../features/torrentMassRename'
import { useI18n } from '../../i18n'
import { MassRenameModeHelp } from './MassRenameModeHelp'
import { ElevatedOverlay } from '../ui/ElevatedOverlay'
import { ResizableSplitPane } from '../ui/ResizableSplitPane'

type TorrentMassRenamePanelProps = {
  scopePath: string
  fileTree: TorrentFileNodeDto[]
  busy?: boolean
  onClose: () => void
  onApply: (operations: { path: string; name: string }[]) => void
}

const PREVIEW_ROW_LIMIT = 200
const RULE_DEBOUNCE_MS = 150

const MODE_VALUES: MassRenameMode[] = [
  'regex',
  'findReplace',
  'prefixSuffix',
  'numbering',
  'template',
]

export function TorrentMassRenamePanel({
  scopePath,
  fileTree,
  busy,
  onClose,
  onApply,
}: TorrentMassRenamePanelProps) {
  const { t } = useI18n()
  const [rule, setRule] = useState<MassRenameRule>(defaultMassRenameRule)
  const modes = useMemo(
    () =>
      MODE_VALUES.map((value) => ({
        value,
        label: t(`massRename.modes.${value}`),
      })),
    [t],
  )

  const scopeFiles = useMemo(
    () => collectScopeFiles(fileTree, scopePath),
    [fileTree, scopePath],
  )

  const debouncedRule = useDebouncedValue(rule, RULE_DEBOUNCE_MS)
  const plan = useMemo(
    () => buildRenamePlan(scopeFiles, debouncedRule),
    [scopeFiles, debouncedRule],
  )
  const ruleErrors = useMemo(
    () => validateMassRenameRule(debouncedRule, scopeFiles),
    [debouncedRule, scopeFiles],
  )
  const validation = useMemo(() => {
    const result = validatePlan(plan)
    const errors = [...ruleErrors, ...result.errors]
    return {
      ...result,
      errors,
      canApply: errors.length === 0 && result.canApply,
    }
  }, [plan, ruleErrors])
  const changedCount = plan.filter((e) => e.changed).length
  const previewRows = plan.slice(0, PREVIEW_ROW_LIMIT)
  const previewHidden = plan.length - previewRows.length

  useEffect(() => {
    setRule(defaultMassRenameRule())
  }, [scopePath])

  const updateRule = (patch: Partial<MassRenameRule>) => {
    setRule((prev) => ({ ...prev, ...patch }))
  }

  const handleApply = () => {
    const operations = plan
      .filter((e) => e.changed)
      .map((e) => ({ path: e.path, name: e.newName }))
    onApply(operations)
  }

  return (
    <ElevatedOverlay
      insetPadding={false}
      onBackdropPointerDown={(e) => {
        if (e.target === e.currentTarget && !busy) onClose()
      }}
    >
        <Box
          w="90vw"
          maxW="90vw"
          h="90vh"
          maxH="90vh"
          bg="surface.panel"
          borderWidth="1px"
          borderColor="border"
          borderRadius="lg"
          boxShadow="2xl"
          display="flex"
          flexDirection="column"
          overflow="hidden"
          onPointerDown={(e) => e.stopPropagation()}
        >
          <Box px={5} pt={4} pb={3} borderBottomWidth="1px" borderColor="border" flexShrink={0}>
            <Text fontWeight="semibold" color="fg" fontSize="md">
              {t('massRename.title')}
            </Text>
            <Text fontSize="xs" color="fg.muted" mt={1}>
              {t('massRename.scopeDetail', {
                label: formatScopeLabel(scopePath),
                count: scopeFiles.length,
              })}
            </Text>
          </Box>

          <ResizableSplitPane
            defaultLeftRatio={2 / 3}
            minLeftRatio={1 / 3}
            minRightRatio={1 / 3}
            left={
            <Box px={5} py={4}>
              <Tabs.Root
                value={rule.mode}
                onValueChange={(e) => updateRule({ mode: e.value as MassRenameMode })}
                variant="line"
                colorPalette="brand"
                size="sm"
              >
                <Tabs.List mb={3} flexWrap="wrap">
                  {modes.map((m) => (
                    <Tabs.Trigger key={m.value} value={m.value}>
                      {m.label}
                    </Tabs.Trigger>
                  ))}
                </Tabs.List>

                <MassRenameModeHelp mode={rule.mode} />

                <Flex gap={4} mb={3} flexWrap="wrap" align="center">
                  <Checkbox.Root
                    checked={rule.stemOnly}
                    disabled={rule.mode === 'regex'}
                    onCheckedChange={(e) => updateRule({ stemOnly: Boolean(e.checked) })}
                  >
                    <Checkbox.HiddenInput />
                    <Checkbox.Control borderColor="border" />
                    <Checkbox.Label fontSize="sm">{t('massRename.stemOnly')}</Checkbox.Label>
                  </Checkbox.Root>
                  <Field.Root w="auto">
                    <Field.Label fontSize="xs">{t('massRename.sort')}</Field.Label>
                    <select
                      value={rule.sort}
                      onChange={(e: ChangeEvent<HTMLSelectElement>) =>
                        updateRule({ sort: e.target.value as MassRenameRule['sort'] })
                      }
                      style={{
                        fontSize: '0.875rem',
                        padding: '4px 8px',
                        borderRadius: '6px',
                        border: '1px solid var(--chakra-colors-border)',
                        background: 'var(--chakra-colors-bg-emphasized)',
                        color: 'var(--chakra-colors-fg)',
                      }}
                    >
                      <option value="path">{t('massRename.sortPath')}</option>
                      <option value="name">{t('massRename.sortName')}</option>
                    </select>
                  </Field.Root>
                </Flex>

                {rule.mode === 'findReplace' && (
                  <Flex direction="column" gap={3}>
                    <Field.Root>
                      <Field.Label>{t('massRename.find')}</Field.Label>
                      <Input
                        value={rule.find}
                        onChange={(e) => updateRule({ find: e.target.value })}
                        bg="bg.emphasized"
                        borderColor="border"
                      />
                    </Field.Root>
                    <Field.Root>
                      <Field.Label>{t('massRename.replace')}</Field.Label>
                      <Input
                        value={rule.replace}
                        onChange={(e) => updateRule({ replace: e.target.value })}
                        bg="bg.emphasized"
                        borderColor="border"
                      />
                    </Field.Root>
                    <Checkbox.Root
                      checked={rule.caseSensitive}
                      onCheckedChange={(e) => updateRule({ caseSensitive: Boolean(e.checked) })}
                    >
                      <Checkbox.HiddenInput />
                      <Checkbox.Control borderColor="border" />
                      <Checkbox.Label fontSize="sm">{t('massRename.caseSensitive')}</Checkbox.Label>
                    </Checkbox.Root>
                  </Flex>
                )}

                {rule.mode === 'prefixSuffix' && (
                  <Flex direction="column" gap={3}>
                    <Field.Root>
                      <Field.Label>{t('massRename.prefix')}</Field.Label>
                      <Input
                        value={rule.prefix}
                        onChange={(e) => updateRule({ prefix: e.target.value })}
                        bg="bg.emphasized"
                        borderColor="border"
                      />
                    </Field.Root>
                    <Field.Root>
                      <Field.Label>{t('massRename.suffix')}</Field.Label>
                      <Input
                        value={rule.suffix}
                        onChange={(e) => updateRule({ suffix: e.target.value })}
                        bg="bg.emphasized"
                        borderColor="border"
                      />
                    </Field.Root>
                  </Flex>
                )}

                {rule.mode === 'numbering' && (
                  <Flex direction="column" gap={3}>
                    <Field.Root>
                      <Field.Label>{t('massRename.template')}</Field.Label>
                      <Input
                        value={rule.numberingTemplate}
                        onChange={(e) => updateRule({ numberingTemplate: e.target.value })}
                        bg="bg.emphasized"
                        borderColor="border"
                        placeholder="{n:02} - {name}"
                      />
                    </Field.Root>
                    <Flex gap={3}>
                      <Field.Root flex="1">
                        <Field.Label>{t('massRename.start')}</Field.Label>
                        <Input
                          type="number"
                          value={rule.numberingStart}
                          onChange={(e) =>
                            updateRule({ numberingStart: Number(e.target.value) || 1 })
                          }
                          bg="bg.emphasized"
                          borderColor="border"
                        />
                      </Field.Root>
                      <Field.Root flex="1">
                        <Field.Label>{t('massRename.step')}</Field.Label>
                        <Input
                          type="number"
                          value={rule.numberingStep}
                          onChange={(e) =>
                            updateRule({ numberingStep: Number(e.target.value) || 1 })
                          }
                          bg="bg.emphasized"
                          borderColor="border"
                        />
                      </Field.Root>
                    </Flex>
                  </Flex>
                )}

                {rule.mode === 'regex' && (
                  <Flex direction="column" gap={3}>
                    <Field.Root>
                      <Field.Label>{t('massRename.pattern')}</Field.Label>
                      <Input
                        value={rule.regexPattern}
                        onChange={(e) => updateRule({ regexPattern: e.target.value })}
                        bg="bg.emphasized"
                        borderColor="border"
                      />
                    </Field.Root>
                    <Field.Root>
                      <Field.Label>{t('massRename.replacement')}</Field.Label>
                      <Input
                        value={rule.regexReplacement}
                        onChange={(e) => updateRule({ regexReplacement: e.target.value })}
                        bg="bg.emphasized"
                        borderColor="border"
                        placeholder="$1 $2.mkv"
                      />
                    </Field.Root>
                    <Field.Root>
                      <Field.Label>{t('massRename.flags')}</Field.Label>
                      <Input
                        value={rule.regexFlags}
                        onChange={(e) => updateRule({ regexFlags: e.target.value })}
                        bg="bg.emphasized"
                        borderColor="border"
                        placeholder="gim"
                      />
                    </Field.Root>
                  </Flex>
                )}

                {rule.mode === 'template' && (
                  <Field.Root>
                    <Field.Label>{t('massRename.template')}</Field.Label>
                    <Input
                      value={rule.template}
                      onChange={(e) => updateRule({ template: e.target.value })}
                      bg="bg.emphasized"
                      borderColor="border"
                      placeholder="{n:02} - {name}{ext}"
                    />
                  </Field.Root>
                )}
              </Tabs.Root>
            </Box>
            }
            right={
            <Box flex="1" minH={0} display="flex" flexDirection="column" px={5} py={4} minW={0}>
              <Text fontSize="sm" fontWeight="medium" color="fg" mb={2} flexShrink={0}>
                {t('massRename.preview')}
                {changedCount > 0 && (
                  <Text as="span" color="brand.500" fontWeight="normal" ml={2}>
                    {t('massRename.changes', { count: changedCount })}
                  </Text>
                )}
              </Text>

              {validation.errors.length > 0 && (
                <Box mb={2} flexShrink={0}>
                  {validation.errors.map((msg) => (
                    <Text key={msg} fontSize="xs" color="red.400">
                      {msg}
                    </Text>
                  ))}
                </Box>
              )}
              {validation.warnings.map((msg) => (
                <Text key={msg} fontSize="xs" color="fg.muted" mb={2} flexShrink={0}>
                  {msg}
                </Text>
              ))}

              <Box
                flex="1"
                minH={0}
                borderWidth="1px"
                borderColor="border"
                borderRadius="md"
                overflow="auto"
                fontSize="xs"
              >
                <Flex
                  px={2}
                  py={1.5}
                  bg="bg.emphasized"
                  borderBottomWidth="1px"
                  borderColor="border"
                  fontWeight="medium"
                  color="fg.muted"
                  position="sticky"
                  top={0}
                  zIndex={1}
                >
                  <Box flex="1">{t('massRename.oldName')}</Box>
                  <Box w="20px" />
                  <Box flex="1">{t('massRename.newName')}</Box>
                </Flex>
                {plan.length === 0 ? (
                  <Text py={4} textAlign="center" color="fg.muted">
                    {t('massRename.noFilesInScope')}
                  </Text>
                ) : (
                  <>
                    {previewRows.map((entry) => (
                      <Flex
                        key={entry.path}
                        px={2}
                        py={1}
                        borderBottomWidth="1px"
                        borderColor="border.muted"
                        color={entry.changed ? 'fg' : 'fg.subtle'}
                        title={entry.path}
                      >
                        <Box flex="1" truncate>
                          {entry.oldName}
                        </Box>
                        <Box w="20px" textAlign="center">
                          →
                        </Box>
                        <Box flex="1" truncate color={entry.changed ? 'brand.500' : undefined}>
                          {entry.newName}
                        </Box>
                      </Flex>
                    ))}
                    {previewHidden > 0 && (
                      <Text py={2} textAlign="center" color="fg.muted">
                        {t('massRename.previewLimited', { count: previewHidden })}
                      </Text>
                    )}
                  </>
                )}
              </Box>
            </Box>
            }
          />

          <Flex
            px={5}
            py={3}
            gap={2}
            justify="flex-end"
            borderTopWidth="1px"
            borderColor="border"
            flexShrink={0}
          >
            <Button variant="outline" borderColor="border" onClick={onClose} disabled={busy}>
              {t('common.cancel')}
            </Button>
            <Button
              colorPalette="brand"
              loading={busy}
              disabled={!validation.canApply}
              onClick={handleApply}
            >
              {t('massRename.applyWithCount', { count: changedCount })}
            </Button>
          </Flex>
        </Box>
    </ElevatedOverlay>
  )
}
