export const MAX_DOWNLOAD_DIR_HISTORY = 50

export function pushDownloadDirHistory(history: readonly string[], path: string): string[] {
  const trimmed = path.trim()
  if (!trimmed) return [...history]

  const next = [trimmed, ...history.filter((item) => item !== trimmed)]
  return next.slice(0, MAX_DOWNLOAD_DIR_HISTORY)
}

export function folderDisplayName(path: string): string {
  const parts = path.replace(/\\/g, '/').split('/').filter(Boolean)
  return parts[parts.length - 1] ?? path
}

export function matchesDownloadDirQuery(path: string, query: string): boolean {
  const q = query.trim().toLowerCase()
  if (!q) return true

  const lower = path.toLowerCase()
  if (lower.includes(q)) return true

  return folderDisplayName(path).toLowerCase().includes(q)
}
