import { useEffect } from 'react'
import { useColorMode } from '../components/ui/color-mode'
import { useApp } from '../context/AppProvider'
import { applyAccentPalette, normalizeAppearance, normalizeColorScheme } from './accentPalettes'

export function ThemeSync() {
  const { settings, settingsLoading } = useApp()
  const { setAppearance, resolvedAppearance } = useColorMode()

  const colorScheme = normalizeColorScheme(settings.ui.colorScheme)
  const appearance = normalizeAppearance(settings.ui.appearance)

  useEffect(() => {
    if (settingsLoading) return
    setAppearance(appearance)
  }, [appearance, setAppearance, settingsLoading])

  useEffect(() => {
    if (settingsLoading) return
    applyAccentPalette(colorScheme, resolvedAppearance)
  }, [colorScheme, resolvedAppearance, settingsLoading])

  useEffect(() => {
    document.documentElement.style.colorScheme = resolvedAppearance
  }, [resolvedAppearance])

  return null
}
