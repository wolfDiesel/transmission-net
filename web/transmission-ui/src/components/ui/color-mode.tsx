import type { ReactNode } from 'react'
import { ThemeProvider, useTheme } from 'next-themes'
import type { AppearanceId } from '../../theme/accentPalettes'

type ColorModeProviderProps = {
  children: ReactNode
}

export function ColorModeProvider({ children }: ColorModeProviderProps) {
  return (
    <ThemeProvider attribute="class" defaultTheme="dark" enableSystem disableTransitionOnChange>
      {children}
    </ThemeProvider>
  )
}

export function useColorMode() {
  const { theme, setTheme, resolvedTheme } = useTheme()

  return {
    appearance: (theme ?? 'dark') as AppearanceId,
    resolvedAppearance: (resolvedTheme ?? 'dark') as 'light' | 'dark',
    setAppearance: setTheme,
  }
}
