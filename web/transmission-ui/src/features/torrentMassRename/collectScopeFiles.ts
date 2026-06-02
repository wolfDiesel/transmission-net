import type { TorrentFileNodeDto } from '../../api/types'
import type { ScopeFile } from './types'
import { splitBasename } from './splitBasename'

export const TORRENT_ROOT_SCOPE = ''

function normalizeScopePath(scopePath: string): string {
  const trimmed = scopePath.trim().replace(/\\/g, '/').replace(/^\/+|\/+$/g, '')
  return trimmed
}

function isInScope(filePath: string, scopePath: string): boolean {
  if (!scopePath) return true
  return filePath === scopePath || filePath.startsWith(`${scopePath}/`)
}

function walk(nodes: TorrentFileNodeDto[], scopePath: string, out: ScopeFile[]) {
  for (const node of nodes) {
    if (node.isFolder) {
      walk(node.children, scopePath, out)
      continue
    }

    if (!isInScope(node.path, scopePath)) continue

    const basename = node.name
    const { stem, ext } = splitBasename(basename)
    const relativePath = scopePath
      ? node.path.slice(scopePath.length + 1)
      : node.path

    out.push({
      path: node.path,
      basename,
      stem,
      ext,
      relativePath,
    })
  }
}

export function collectScopeFiles(
  tree: TorrentFileNodeDto[],
  scopePath: string,
): ScopeFile[] {
  const normalizedScope = normalizeScopePath(scopePath)
  const files: ScopeFile[] = []
  walk(tree, normalizedScope, files)
  return files.sort((a, b) => a.path.localeCompare(b.path))
}

export function formatScopeLabel(scopePath: string): string {
  const normalized = normalizeScopePath(scopePath)
  return normalized ? `${normalized}/` : 'All files'
}
