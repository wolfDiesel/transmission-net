import { Box } from '@chakra-ui/react'
import type { TorrentDetailsDto } from '../../api/types'
import {
  formatPercent,
  formatSize,
  formatUnixDate,
  statusLabel,
} from '../../utils/format'
import { PropertyRow } from './PropertyRow'

type TorrentDetailsGeneralTabProps = {
  details: TorrentDetailsDto
}

export function TorrentDetailsGeneralTab({ details }: TorrentDetailsGeneralTabProps) {
  return (
    <Box>
      <PropertyRow label="Name" value={details.name} />
      <PropertyRow label="Status" value={statusLabel(details.status)} />
      <PropertyRow label="Progress" value={formatPercent(details.percentDone)} />
      <PropertyRow label="Size" value={formatSize(details.totalSize)} />
      <PropertyRow label="Download directory" value={details.downloadDir || '—'} />
      <PropertyRow label="Priority" value={details.bandwidthPriority} />
      <PropertyRow label="Queue position" value={details.queuePosition} />
      <PropertyRow label="Added" value={formatUnixDate(details.addedDate)} />
      <PropertyRow label="Started" value={formatUnixDate(details.startDate)} />
      <PropertyRow label="Completed" value={formatUnixDate(details.doneDate)} />
      <PropertyRow label="Created" value={formatUnixDate(details.dateCreated)} />
      <PropertyRow label="Creator" value={details.creator || '—'} />
      <PropertyRow label="Comment" value={details.comment || '—'} />
      <PropertyRow label="Hash" value={details.hashString || '—'} />
      <PropertyRow label="Piece size" value={formatSize(details.pieceSize)} />
      <PropertyRow label="Private" value={details.isPrivate ? 'Yes' : 'No'} />
      {details.error !== 0 && (
        <PropertyRow
          label="Error"
          value={details.errorString || `Code ${details.error}`}
        />
      )}
    </Box>
  )
}
