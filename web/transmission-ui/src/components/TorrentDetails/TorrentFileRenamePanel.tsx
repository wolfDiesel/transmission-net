import { Box, Button, Field, Flex, Input, Text } from '@chakra-ui/react'
import { useEffect, useState } from 'react'
import type { TorrentFileNodeDto } from '../../api/types'
import { useI18n } from '../../i18n'
import { ElevatedOverlay } from '../ui/ElevatedOverlay'

type TorrentFileRenamePanelProps = {
  node: TorrentFileNodeDto
  busy?: boolean
  onClose: () => void
  onConfirm: (path: string, name: string) => void
}

export function TorrentFileRenamePanel({
  node,
  busy,
  onClose,
  onConfirm,
}: TorrentFileRenamePanelProps) {
  const { t } = useI18n()
  const [name, setName] = useState(node.name)

  useEffect(() => {
    setName(node.name)
  }, [node.path, node.name])

  const trimmed = name.trim()
  const invalid = trimmed.length === 0 || trimmed.includes('/') || trimmed.includes('\\')
  const unchanged = trimmed === node.name

  return (
    <ElevatedOverlay
      onBackdropPointerDown={(e) => {
        if (e.target === e.currentTarget && !busy) onClose()
      }}
    >
        <Box
          w="full"
          maxW="440px"
          bg="surface.panel"
          borderWidth="1px"
          borderColor="border"
          borderRadius="lg"
          boxShadow="2xl"
          onPointerDown={(e) => e.stopPropagation()}
        >
          <Box px={5} pt={4} pb={3} borderBottomWidth="1px" borderColor="border">
            <Text fontWeight="semibold" color="fg">
              {t('torrentDetails.renamePanel.title')}
            </Text>
          </Box>
          <Box px={5} py={4}>
            <Text fontSize="xs" color="fg.muted" mb={3} lineClamp={2} title={node.path}>
              {node.path}
            </Text>
            <Field.Root>
              <Field.Label>{t('torrentDetails.renamePanel.newName')}</Field.Label>
              <Input
                value={name}
                onChange={(e) => setName(e.target.value)}
                bg="bg.emphasized"
                borderColor="border"
                autoFocus
                onKeyDown={(e) => {
                  if (e.key === 'Escape' && !busy) onClose()
                  if (e.key === 'Enter' && !invalid && !unchanged && !busy) {
                    onConfirm(node.path, trimmed)
                  }
                }}
              />
            </Field.Root>
          </Box>
          <Flex
            px={5}
            py={3}
            gap={2}
            justify="flex-end"
            borderTopWidth="1px"
            borderColor="border"
          >
            <Button variant="outline" borderColor="border" onClick={onClose} disabled={busy}>
              {t('common.cancel')}
            </Button>
            <Button
              colorPalette="brand"
              loading={busy}
              disabled={invalid || unchanged}
              onClick={() => onConfirm(node.path, trimmed)}
            >
              {t('common.rename')}
            </Button>
          </Flex>
        </Box>
    </ElevatedOverlay>
  )
}
