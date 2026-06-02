import type { ReactNode } from 'react'
import { appToaster } from './toaster'

export type AppToastVariant = 'success' | 'error' | 'info'

export function showAppToast(options: {
  title: string
  description?: string
  variant?: AppToastVariant
  icon?: ReactNode
  duration?: number
}) {
  const variant = options.variant ?? 'info'

  appToaster.create({
    title: options.title,
    description: options.description,
    type: variant === 'error' ? 'error' : variant === 'success' ? 'success' : 'info',
    duration: options.duration ?? 4000,
    meta: {
      variant,
      icon: options.icon,
    },
  })
}
