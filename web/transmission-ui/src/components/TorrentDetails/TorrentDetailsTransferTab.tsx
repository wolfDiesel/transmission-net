import { Box } from '@chakra-ui/react'
import type { TorrentDetailsDto } from '../../api/types'
import {
  formatBytesPerSec,
  formatEta,
  formatRatio,
  formatSize,
} from '../../utils/format'
import { PropertyRow } from './PropertyRow'

type TorrentDetailsTransferTabProps = {
  details: TorrentDetailsDto
}

export function TorrentDetailsTransferTab({ details }: TorrentDetailsTransferTabProps) {
  return (
    <Box>
      <PropertyRow label="Download speed" value={formatBytesPerSec(details.rateDownload)} />
      <PropertyRow label="Upload speed" value={formatBytesPerSec(details.rateUpload)} />
      <PropertyRow label="ETA" value={formatEta(details.eta)} />
      <PropertyRow label="Left" value={formatSize(details.leftUntilDone)} />
      <PropertyRow label="Downloaded" value={formatSize(details.downloadedEver)} />
      <PropertyRow label="Uploaded" value={formatSize(details.uploadedEver)} />
      <PropertyRow label="Ratio" value={formatRatio(details.uploadRatio)} />
      <PropertyRow label="Peers" value={details.peersConnected} />
    </Box>
  )
}
