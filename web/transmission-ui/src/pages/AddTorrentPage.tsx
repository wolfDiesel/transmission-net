import {
  Box,
  Button,
  Checkbox,
  Field,
  Flex,
  Input,
  Spinner,
  Text,
} from '@chakra-ui/react'
import { useCallback, useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, ApiError } from '../api/client'
import type { TorrentMetainfoPreviewDto } from '../api/types'
import { DownloadDirCombobox } from '../components/AddTorrent/DownloadDirCombobox'
import { showAppToast } from '../components/AppToast'
import { TorrentFileTree } from '../components/TorrentDetails/TorrentFileTree'
import { useApp } from '../context/AppProvider'
import { useDownloadDirHistory } from '../hooks/useDownloadDirHistory'
import { readFileAsBase64 } from '../utils/readFileAsBase64'
import { formatSize } from '../utils/format'

export function AddTorrentPage() {
  const navigate = useNavigate()
  const { settingsLoading } = useApp()
  const { directories, remember } = useDownloadDirHistory()
  const [downloadDir, setDownloadDir] = useState('')
  const initialDirSet = useRef(false)
  const [paused, setPaused] = useState(false)
  const [metainfoBase64, setMetainfoBase64] = useState<string | null>(null)
  const [preview, setPreview] = useState<TorrentMetainfoPreviewDto | null>(null)
  const [inspectError, setInspectError] = useState<string | null>(null)
  const [inspecting, setInspecting] = useState(false)
  const [adding, setAdding] = useState(false)
  const [sessionDir, setSessionDir] = useState<string | null>(null)
  const [sessionLoading, setSessionLoading] = useState(true)

  useEffect(() => {
    let cancelled = false
    void api
      .getDaemonSessionSettings()
      .then((session) => {
        if (!cancelled) setSessionDir(session.downloadDir)
      })
      .catch(() => {
        if (!cancelled) setSessionDir('')
      })
      .finally(() => {
        if (!cancelled) setSessionLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [])

  useEffect(() => {
    if (initialDirSet.current || sessionLoading || settingsLoading || sessionDir === null) return
    const preferred = directories[0]?.trim() || sessionDir.trim()
    setDownloadDir(preferred)
    initialDirSet.current = true
  }, [directories, sessionDir, sessionLoading, settingsLoading])

  const handleFileChange = useCallback(async (file: File | null) => {
    setInspectError(null)
    setPreview(null)
    setMetainfoBase64(null)

    if (!file) return

    if (!file.name.toLowerCase().endsWith('.torrent')) {
      setInspectError('Select a .torrent file')
      return
    }

    setInspecting(true)
    try {
      const base64 = await readFileAsBase64(file)
      const loaded = await api.inspectTorrentMetainfo(base64)
      setMetainfoBase64(base64)
      setPreview(loaded)
    } catch (e) {
      setInspectError(e instanceof ApiError ? e.message : 'Failed to read torrent file')
    } finally {
      setInspecting(false)
    }
  }, [])

  const handleAdd = async () => {
    if (!metainfoBase64 || !preview) return

    const dir = downloadDir.trim()
    if (!dir) {
      showAppToast({ title: 'Download directory is required', variant: 'error' })
      return
    }

    setAdding(true)
    try {
      const result = await api.addTorrent({
        metainfoBase64,
        downloadDir: dir,
        paused,
      })
      remember(dir)
      showAppToast({ title: `Added: ${result.name}`, variant: 'success' })
      navigate('/')
    } catch (e) {
      showAppToast({
        title: e instanceof ApiError ? e.message : 'Failed to add torrent',
        variant: 'error',
      })
    } finally {
      setAdding(false)
    }
  }

  return (
    <Box display="flex" flexDirection="column" flex="1" minH={0} gap={4}>
      <Box>
        <Text fontSize="lg" fontWeight="semibold" color="fg">
          Add torrent
        </Text>
        <Text fontSize="xs" color="fg.muted">
          Choose a .torrent file to inspect contents before adding to the daemon
        </Text>
      </Box>

      <Field.Root>
        <Field.Label>Torrent file</Field.Label>
        <Input
          type="file"
          accept=".torrent,application/x-bittorrent"
          bg="bg.emphasized"
          borderColor="border"
          py={1}
          onChange={(e) => {
            const file = e.target.files?.[0] ?? null
            void handleFileChange(file)
          }}
        />
        {inspectError && (
          <Field.HelperText color="red.400" fontSize="sm">
            {inspectError}
          </Field.HelperText>
        )}
      </Field.Root>

      {inspecting && (
        <Flex justify="center" py={8}>
          <Spinner color="brand.500" />
        </Flex>
      )}

      {preview && !inspecting && (
        <Flex direction="column" gap={4} flex="1" minH={0}>
          <Box
            borderWidth="1px"
            borderColor="border"
            borderRadius="md"
            px={4}
            py={3}
            bg="bg.emphasized"
          >
            <Text fontWeight="semibold" color="fg" mb={1}>
              {preview.name}
            </Text>
            <Text fontSize="sm" color="fg.muted">
              File: {preview.fileName} · Total size: {formatSize(preview.totalSize)}
            </Text>
          </Box>

          <Field.Root w="full">
            <Field.Label>Download directory</Field.Label>
            <DownloadDirCombobox
              value={downloadDir}
              onChange={setDownloadDir}
              directories={directories}
              disabled={sessionLoading || settingsLoading}
            />
          </Field.Root>

          <Checkbox.Root
            checked={paused}
            onCheckedChange={(e) => setPaused(Boolean(e.checked))}
          >
            <Checkbox.HiddenInput />
            <Checkbox.Control borderColor="border" />
            <Checkbox.Label fontSize="sm">Add paused (do not start immediately)</Checkbox.Label>
          </Checkbox.Root>

          <Box flex="1" minH={0} display="flex" flexDirection="column">
            <Text fontSize="sm" fontWeight="medium" color="fg" mb={2}>
              Files in torrent
            </Text>
            <Box
              flex="1"
              minH={0}
              overflow="auto"
              borderWidth="1px"
              borderColor="border"
              borderRadius="md"
              px={2}
              py={2}
            >
              <TorrentFileTree nodes={preview.fileTree} torrentName={preview.name} />
            </Box>
          </Box>

          <Flex justify="flex-end" gap={2} flexShrink={0}>
            <Button
              variant="outline"
              borderColor="border"
              onClick={() => {
                setPreview(null)
                setMetainfoBase64(null)
                setInspectError(null)
              }}
            >
              Clear
            </Button>
            <Button
              colorPalette="brand"
              loading={adding}
              onClick={() => void handleAdd()}
            >
              Add torrent
            </Button>
          </Flex>
        </Flex>
      )}
    </Box>
  )
}
