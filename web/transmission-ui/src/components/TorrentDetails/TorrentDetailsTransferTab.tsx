import { Box } from '@chakra-ui/react'
import type { TorrentDetailsDto } from '../../api/types'
import { useI18n } from '../../i18n'
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
  const { t } = useI18n()
  const p = (key: string) => t(`torrentDetails.properties.${key}`)

  return (
    <Box>
      <PropertyRow label={p('downloadSpeed')} value={formatBytesPerSec(details.rateDownload)} />
      <PropertyRow label={p('uploadSpeed')} value={formatBytesPerSec(details.rateUpload)} />
      <PropertyRow label={p('eta')} value={formatEta(details.eta)} />
      <PropertyRow label={p('left')} value={formatSize(details.leftUntilDone)} />
      <PropertyRow label={p('downloaded')} value={formatSize(details.downloadedEver)} />
      <PropertyRow label={p('uploaded')} value={formatSize(details.uploadedEver)} />
      <PropertyRow label={p('ratio')} value={formatRatio(details.uploadRatio)} />
      <PropertyRow label={p('peers')} value={details.peersConnected} />
    </Box>
  )
}
