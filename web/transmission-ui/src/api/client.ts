import type {
  AppSettingsDto,
  DaemonConnectionDto,
  DaemonSessionSettingsDto,
  DaemonStatusDto,
  TorrentActionRequest,
  TorrentBandwidthPriority,
  TorrentDetailsDto,
  TorrentDto,
  TorrentFileNodeDto,
  TorrentAddRequestDto,
  TorrentAddResultDto,
  TorrentMetainfoPreviewDto,
  TorrentRenameBatchResultDto,
  TorrentRenameOperationDto,
} from './types'

export class ApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.status = status
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...init?.headers,
    },
  })

  if (!response.ok) {
    const body = (await response.json().catch(() => null)) as { error?: string } | null
    throw new ApiError(body?.error ?? response.statusText, response.status)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

export const api = {
  getHealth: () => request<{ status: string }>('/api/health'),
  getSettings: () => request<AppSettingsDto>('/api/settings'),
  saveSettings: (settings: AppSettingsDto) =>
    request<AppSettingsDto>('/api/settings', {
      method: 'PUT',
      body: JSON.stringify(settings),
    }),
  testConnection: (daemon: DaemonConnectionDto) =>
    request<{ status: string }>('/api/connection/test', {
      method: 'POST',
      body: JSON.stringify(daemon),
    }),
  getTorrents: async () => {
    const list = await request<TorrentDto[]>('/api/torrents')
    return list.map(normalizeTorrent)
  },
  getTorrentDetails: async (id: number) => {
    const raw = await request<TorrentDetailsDto & { BandwidthPriority?: string }>(
      `/api/torrents/${id}`,
    )
    return normalizeTorrentDetails(raw)
  },
  executeTorrentAction: (body: TorrentActionRequest) =>
    request<{ status: string }>('/api/torrents/actions', {
      method: 'POST',
      body: JSON.stringify(body),
    }),
  renameTorrentBatch: (torrentId: number, operations: TorrentRenameOperationDto[]) =>
    request<TorrentRenameBatchResultDto>(`/api/torrents/${torrentId}/rename-batch`, {
      method: 'POST',
      body: JSON.stringify({ operations }),
    }),
  getDaemonSessionSettings: () =>
    request<DaemonSessionSettingsDto>('/api/daemon/session-settings'),
  saveDaemonSessionSettings: (settings: DaemonSessionSettingsDto) =>
    request<DaemonSessionSettingsDto>('/api/daemon/session-settings', {
      method: 'PUT',
      body: JSON.stringify(settings),
    }),
  inspectTorrentMetainfo: (metainfoBase64: string) =>
    request<TorrentMetainfoPreviewDto>('/api/torrents/inspect', {
      method: 'POST',
      body: JSON.stringify({ metainfoBase64 }),
    }),
  addTorrent: (body: TorrentAddRequestDto) =>
    request<TorrentAddResultDto>('/api/torrents/add', {
      method: 'POST',
      body: JSON.stringify(body),
    }),
  getStatus: async (options?: { counts?: boolean }) => {
    const query = options?.counts === false ? '?counts=false' : ''
    const raw = await request<Record<string, unknown>>(`/api/status${query}`)
    return normalizeDaemonStatus(raw)
  },
}

function normalizeTorrentDetails(
  raw: TorrentDetailsDto & { BandwidthPriority?: string },
): TorrentDetailsDto {
  const base = normalizeTorrent(raw)
  return {
    ...base,
    error: Number(raw.error ?? 0),
    errorString: String(raw.errorString ?? ''),
    comment: String(raw.comment ?? ''),
    creator: String(raw.creator ?? ''),
    dateCreated: Number(raw.dateCreated ?? 0),
    hashString: String(raw.hashString ?? ''),
    pieceSize: Number(raw.pieceSize ?? 0),
    isPrivate: Boolean(raw.isPrivate ?? false),
    fileTree: normalizeFileTree(raw.fileTree ?? []),
  }
}

function normalizeFileTree(nodes: TorrentFileNodeDto[]): TorrentFileNodeDto[] {
  return nodes.map((node) => ({
    name: node.name,
    path: node.path,
    isFolder: Boolean(node.isFolder),
    fileIndex: node.fileIndex ?? null,
    length: Number(node.length ?? 0),
    bytesCompleted: Number(node.bytesCompleted ?? 0),
    wanted: node.wanted ?? null,
    priority: node.priority ?? null,
    children: normalizeFileTree(node.children ?? []),
  }))
}

function normalizeTorrent(raw: TorrentDto & { BandwidthPriority?: string }): TorrentDto {
  const priority = raw.bandwidthPriority ?? raw.BandwidthPriority
  return {
    ...raw,
    bandwidthPriority: normalizeBandwidthPriority(priority),
  }
}

function normalizeBandwidthPriority(value: string | undefined): TorrentBandwidthPriority {
  if (value === 'low' || value === 'high' || value === 'normal') return value
  return 'normal'
}

function normalizeDaemonStatus(raw: Record<string, unknown>): DaemonStatusDto {
  return {
    connected: Boolean(raw.connected ?? raw.Connected),
    downloadSpeed: Number(raw.downloadSpeed ?? raw.DownloadSpeed ?? 0),
    uploadSpeed: Number(raw.uploadSpeed ?? raw.UploadSpeed ?? 0),
    downloadingCount: Number(raw.downloadingCount ?? raw.DownloadingCount ?? 0),
    completedCount: Number(raw.completedCount ?? raw.CompletedCount ?? 0),
  }
}
