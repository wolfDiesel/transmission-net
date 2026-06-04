export interface DaemonConnectionDto {
  host: string
  port: number
  rpcPath: string
  username: string
  password: string | null
}

export interface TorrentTableColumnSettingDto {
  id: string
  visible: boolean
  widthPx?: number | null
}

export interface TorrentTableSettingsDto {
  columns: TorrentTableColumnSettingDto[]
  sortColumnId: string
  sortDescending: boolean
}

export type TorrentFileAssociationPromptStatus = 'not_asked' | 'registered' | 'declined'

export interface UiSettingsDto {
  refreshIntervalSeconds: number
  windowWidth: number
  windowHeight: number
  torrentTable: TorrentTableSettingsDto
  colorScheme: string
  appearance: string
  downloadDirHistory?: string[]
  torrentFileAssociation?: TorrentFileAssociationPromptStatus
  trayEnabled?: boolean
  minimizeToTray?: boolean
  closeToTray?: boolean
  language?: string
}

export interface DesktopCapabilitiesDto {
  traySupported: boolean
  traySettingsAvailable: boolean
}

export interface TorrentFileAssociationStatusDto {
  isSupported: boolean
  hasDesktopEntry: boolean
  isDefaultHandler: boolean
  promptStatus: TorrentFileAssociationPromptStatus
  shouldPrompt: boolean
}

export interface PendingTorrentPathDto {
  path: string | null
}

export interface AppSettingsDto {
  daemon: DaemonConnectionDto
  ui: UiSettingsDto
}

export interface DaemonSessionSettingsDto {
  downloadDir: string
  incompleteDir: string
  incompleteDirEnabled: boolean
  trashOriginalTorrentFiles: boolean
  peerLimitGlobal: number
  peerLimitPerTorrent: number
  speedLimitDownKbps: number
  speedLimitUpKbps: number
  speedLimitDownEnabled: boolean
  speedLimitUpEnabled: boolean
  seedRatioLimit: number
  seedRatioLimited: boolean
  idleSeedingLimitMinutes: number
  idleSeedingLimitEnabled: boolean
}

export interface DaemonStatusDto {
  connected: boolean
  downloadSpeed: number
  uploadSpeed: number
  downloadingCount: number
  completedCount: number
}

export enum TorrentStatus {
  Stopped = 0,
  CheckWait = 1,
  Checking = 2,
  DownloadWait = 3,
  Downloading = 4,
  SeedWait = 5,
  Seeding = 6,
  Unknown = -1,
}

export type TorrentBandwidthPriority = 'low' | 'normal' | 'high'

export type TorrentActionName =
  | 'start'
  | 'stop'
  | 'remove'
  | 'verify'
  | 'set-priority'
  | 'move'
  | 'rename-path'

export interface TorrentActionRequest {
  action: TorrentActionName
  ids: number[]
  deleteLocalData?: boolean
  priority?: TorrentBandwidthPriority
  location?: string
  move?: boolean
  path?: string
  name?: string
}

export interface TorrentDto {
  id: number
  name: string
  status: TorrentStatus
  percentDone: number
  rateDownload: number
  rateUpload: number
  eta: number
  totalSize: number
  addedDate: number
  doneDate: number
  startDate: number
  uploadRatio: number
  peersConnected: number
  leftUntilDone: number
  downloadedEver: number
  uploadedEver: number
  queuePosition: number
  downloadDir: string
  bandwidthPriority: TorrentBandwidthPriority
}

export interface TorrentFileNodeDto {
  name: string
  path: string
  isFolder: boolean
  fileIndex: number | null
  length: number
  bytesCompleted: number
  wanted: boolean | null
  priority: number | null
  children: TorrentFileNodeDto[]
}

export interface TorrentRenameOperationDto {
  path: string
  name: string
}

export interface TorrentRenameBatchResultDto {
  applied: number
  failures: { path: string; error: string }[]
}

export interface TorrentMetainfoPreviewDto {
  name: string
  fileName: string
  totalSize: number
  fileTree: TorrentFileNodeDto[]
}

export interface TorrentMetainfoFromPathDto {
  metainfoBase64: string
  preview: TorrentMetainfoPreviewDto
}

export interface TorrentAddRequestDto {
  metainfoBase64: string
  downloadDir?: string | null
  paused?: boolean
}

export interface TorrentAddResultDto {
  id: number
  name: string
  hashString: string
}

export interface TorrentDetailsDto extends TorrentDto {
  error: number
  errorString: string
  comment: string
  creator: string
  dateCreated: number
  hashString: string
  pieceSize: number
  isPrivate: boolean
  fileTree: TorrentFileNodeDto[]
}
