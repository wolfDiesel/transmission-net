import { Box } from '@chakra-ui/react'
import type { TorrentDetailsDto } from '../../api/types'
import { useI18n } from '../../i18n'
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
  const { t } = useI18n()
  const p = (key: string) => t(`torrentDetails.properties.${key}`)

  return (
    <Box>
      <PropertyRow label={p('name')} value={details.name} />
      <PropertyRow label={p('status')} value={statusLabel(details.status)} />
      <PropertyRow label={p('progress')} value={formatPercent(details.percentDone)} />
      <PropertyRow label={p('size')} value={formatSize(details.totalSize)} />
      <PropertyRow label={p('downloadDir')} value={details.downloadDir || '—'} />
      <PropertyRow label={p('priority')} value={details.bandwidthPriority} />
      <PropertyRow label={p('queuePosition')} value={details.queuePosition} />
      <PropertyRow label={p('added')} value={formatUnixDate(details.addedDate)} />
      <PropertyRow label={p('started')} value={formatUnixDate(details.startDate)} />
      <PropertyRow label={p('completed')} value={formatUnixDate(details.doneDate)} />
      <PropertyRow label={p('created')} value={formatUnixDate(details.dateCreated)} />
      <PropertyRow label={p('creator')} value={details.creator || '—'} />
      <PropertyRow label={p('comment')} value={details.comment || '—'} />
      <PropertyRow label={p('hash')} value={details.hashString || '—'} />
      <PropertyRow label={p('pieceSize')} value={formatSize(details.pieceSize)} />
      <PropertyRow
        label={p('private')}
        value={details.isPrivate ? t('torrentDetails.properties.yes') : t('torrentDetails.properties.no')}
      />
      {details.error !== 0 && (
        <PropertyRow
          label={p('error')}
          value={details.errorString || `Code ${details.error}`}
        />
      )}
    </Box>
  )
}
