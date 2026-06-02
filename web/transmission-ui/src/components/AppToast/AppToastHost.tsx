import { Stack, Toast, Toaster } from '@chakra-ui/react'
import type { ReactNode } from 'react'
import { appToaster } from './toaster'
import type { AppToastVariant } from './showAppToast'

function ToastIcon({ variant }: { variant: AppToastVariant }) {
  const paths: Record<AppToastVariant, ReactNode> = {
    success: (
      <path
        fill="currentColor"
        d="M10 2a8 8 0 1 0 0 16 8 8 0 0 0 0-16Zm3.7 5.3a1 1 0 0 1 0 1.4l-4 4a1 1 0 0 1-1.4 0l-2-2a1 1 0 1 1 1.4-1.4l1.3 1.3 3.3-3.3a1 1 0 0 1 1.4 0Z"
      />
    ),
    error: (
      <path
        fill="currentColor"
        d="M10 2a8 8 0 1 0 0 16 8 8 0 0 0 0-16ZM9 6a1 1 0 1 1 2 0v4a1 1 0 1 1-2 0V6Zm1 9a1.25 1.25 0 1 1 0-2.5 1.25 1.25 0 0 1 0 2.5Z"
      />
    ),
    info: (
      <path
        fill="currentColor"
        d="M10 2a8 8 0 1 0 0 16 8 8 0 0 0 0-16ZM9 8a1 1 0 1 1 2 0v4a1 1 0 0 1-2 0V8Zm1 7a1.25 1.25 0 1 1 0-2.5 1.25 1.25 0 0 1 0 2.5Z"
      />
    ),
  }

  return (
    <svg
      viewBox="0 0 20 20"
      width={20}
      height={20}
      fill="#FF7800"
      style={{ flexShrink: 0, marginTop: 2 }}
    >
      {paths[variant]}
    </svg>
  )
}

export function AppToastHost() {
  return (
    <Toaster toaster={appToaster} insetInline={{ md: '4' }} top="4">
      {(toast) => {
        const variant = (toast.meta?.variant as AppToastVariant | undefined) ?? 'info'

        return (
          <Toast.Root
            width={{ base: 'sm', md: 'md' }}
            bg="#111111"
            borderWidth="1px"
            borderColor="#FF7800"
            borderRadius="md"
            boxShadow="0 8px 24px rgba(0,0,0,0.45)"
            color="gray.100"
            display="flex"
            alignItems="flex-start"
            gap={3}
            p={3}
          >
            <ToastIcon variant={variant} />
            <Stack gap={0.5} flex="1" minW={0}>
              <Toast.Title fontWeight="semibold" color="#FF7800">
                {toast.title}
              </Toast.Title>
              {toast.description && (
                <Toast.Description color="gray.300" fontSize="sm">
                  {toast.description}
                </Toast.Description>
              )}
            </Stack>
            <Toast.CloseTrigger color="gray.400" _hover={{ color: '#FF7800' }} />
          </Toast.Root>
        )
      }}
    </Toaster>
  )
}
