import type { TorrentDto } from '../../api/types'

function displaySnapshot(t: TorrentDto) {
  return [
    t.id,
    t.name,
    t.status,
    t.percentDone,
    t.rateDownload,
    t.rateUpload,
    t.eta,
    t.totalSize,
    t.addedDate,
    t.doneDate,
    t.startDate,
    t.uploadRatio,
    t.peersConnected,
    t.leftUntilDone,
    t.downloadedEver,
    t.uploadedEver,
    t.queuePosition,
    t.downloadDir,
    t.bandwidthPriority,
  ].join('\0')
}

export function mergeTorrentList(prev: TorrentDto[], next: TorrentDto[]): TorrentDto[] {
  if (prev.length !== next.length) return next

  const nextIds = new Set(next.map((t) => t.id))
  if (prev.some((t) => !nextIds.has(t.id))) return next

  const prevById = new Map(prev.map((t) => [t.id, t]))
  const merged: TorrentDto[] = []
  let sameRefs = true

  for (const item of next) {
    const previous = prevById.get(item.id)
    if (!previous) return next
    if (displaySnapshot(previous) === displaySnapshot(item)) {
      merged.push(previous)
    } else {
      merged.push(item)
      sameRefs = false
    }
  }

  if (sameRefs && merged.every((t, i) => t === prev[i])) return prev
  return merged
}
