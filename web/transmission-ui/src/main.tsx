import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { ChakraProvider } from '@chakra-ui/react'
import { App } from './App'
import { ColorModeProvider } from './components/ui/color-mode'
import { applyAccentPalette, DEFAULT_COLOR_SCHEME } from './theme/accentPalettes'
import { system } from './theme'
import { ensureOverlayRoot } from './lib/overlayRoot'

applyAccentPalette(DEFAULT_COLOR_SCHEME)
ensureOverlayRoot()

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ColorModeProvider>
      <ChakraProvider value={system}>
        <App />
      </ChakraProvider>
    </ColorModeProvider>
  </StrictMode>,
)
