import { Box, Button, IconButton, Input, Text } from '@chakra-ui/react'
import { useMemo, useState } from 'react'
import { folderDisplayName, matchesDownloadDirQuery } from '../../features/downloadDir/downloadDirHistory'

type DownloadDirComboboxProps = {
  value: string
  onChange: (value: string) => void
  directories: readonly string[]
  disabled?: boolean
  placeholder?: string
}

const listStyles = {
  bg: 'bg.emphasized',
  borderColor: 'border',
  borderWidth: '1px',
  borderRadius: 'md',
  boxShadow: 'lg',
  maxH: '240px',
  overflowY: 'auto' as const,
  zIndex: 'popover',
}

const itemHoverBg = 'color-mix(in srgb, var(--app-brand-500, #F07818) 14%, var(--chakra-colors-surface-raised, #1A1A1A))'

function ChevronDownIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M6 9l6 6 6-6"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  )
}

export function DownloadDirCombobox({
  value,
  onChange,
  directories,
  disabled,
  placeholder = '/path/to/downloads',
}: DownloadDirComboboxProps) {
  const [open, setOpen] = useState(false)

  const suggestions = useMemo(
    () => directories.filter((path) => matchesDownloadDirQuery(path, value)).slice(0, 40),
    [directories, value],
  )

  const canShowList = open && !disabled && suggestions.length > 0

  return (
    <Box w="full">
      <Box position="relative" w="full">
        <Input
          w="full"
          value={value}
          onChange={(e) => onChange(e.target.value)}
          onFocus={() => setOpen(true)}
          onBlur={() => setOpen(false)}
          disabled={disabled}
          bg="bg.emphasized"
          borderColor="border"
          placeholder={placeholder}
          autoComplete="off"
          pr="2.5rem"
          title={value || placeholder}
        />
        <IconButton
          aria-label="Show recent download folders"
          variant="ghost"
          size="sm"
          position="absolute"
          right={1}
          top="50%"
          transform="translateY(-50%)"
          color="fg.muted"
          disabled={disabled || directories.length === 0}
          onMouseDown={(e) => e.preventDefault()}
          onClick={() => setOpen((prev) => !prev)}
        >
          <ChevronDownIcon />
        </IconButton>
      </Box>
      {canShowList && (
        <Box position="relative" w="full">
          <Box position="absolute" top={0} left={0} right={0} {...listStyles}>
            {suggestions.map((path) => (
              <Button
                key={path}
                variant="ghost"
                display="block"
                w="full"
                h="auto"
                px={3}
                py={2}
                justifyContent="flex-start"
                fontWeight="normal"
                borderRadius={0}
                color="fg"
                _hover={{ bg: itemHoverBg }}
                onMouseDown={(e) => e.preventDefault()}
                onClick={() => {
                  onChange(path)
                  setOpen(false)
                }}
              >
                <Text fontSize="sm" fontWeight="medium" truncate>
                  {folderDisplayName(path)}
                </Text>
                {folderDisplayName(path) !== path && (
                  <Text fontSize="xs" color="fg.muted" truncate title={path}>
                    {path}
                  </Text>
                )}
              </Button>
            ))}
          </Box>
        </Box>
      )}
      <Text fontSize="xs" color="fg.muted" mt={1}>
        {directories.length > 0
          ? 'Type to filter recent folders or pick from the list'
          : 'Enter a path; recent folders are saved after you add torrents'}
      </Text>
    </Box>
  )
}
