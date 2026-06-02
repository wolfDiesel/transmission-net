import { createSystem, defaultConfig, defineConfig } from '@chakra-ui/react'

const appConfig = defineConfig({
  globalCss: {
    'html, body, #root': {
      height: '100%',
      margin: 0,
    },
    body: {
      bg: 'surface.canvas',
      color: 'fg',
    },
    '.light': {
      colorScheme: 'light',
    },
    '.dark': {
      colorScheme: 'dark',
    },
    '.light *': {
      scrollbarWidth: 'thin',
      scrollbarColor: 'rgba(0, 0, 0, 0.28) transparent',
    },
    '.dark *': {
      scrollbarWidth: 'thin',
      scrollbarColor: 'rgba(255, 255, 255, 0.25) transparent',
    },
    '.light *::-webkit-scrollbar-thumb': {
      backgroundColor: 'rgba(0, 0, 0, 0.22)',
      borderRadius: '9999px',
    },
    '.dark *::-webkit-scrollbar-thumb': {
      backgroundColor: 'rgba(255, 255, 255, 0.22)',
      borderRadius: '9999px',
    },
    '*::-webkit-scrollbar': {
      width: '8px',
      height: '8px',
    },
    '*::-webkit-scrollbar-track': {
      backgroundColor: 'transparent',
    },
    '.chakra-table__header th': {
      backgroundColor: 'transparent',
      color: '{colors.fg.muted}',
      fontWeight: '500',
      textTransform: 'none',
      letterSpacing: 'normal',
    },
    '.chakra-table__header tr': {
      backgroundColor: 'transparent',
    },
    '.chakra-table tbody tr': {
      borderColor: '{colors.border.muted}',
    },
  },
  theme: {
    tokens: {
      colors: {
        brand: {
          500: { value: 'var(--app-brand-500, #F07818)' },
          600: { value: 'var(--app-brand-600, #E06810)' },
        },
      },
    },
    semanticTokens: {
      shadows: {
        island: {
          value: {
            _light: '0 4px 18px rgba(0, 0, 0, 0.08), 0 0 0 1px rgba(0, 0, 0, 0.06)',
            _dark: '0 6px 28px rgba(0, 0, 0, 0.42), 0 0 0 1px rgba(255, 255, 255, 0.05)',
          },
        },
      },
      colors: {
        surface: {
          canvas: {
            value: {
              _light: '#FAFAFA',
              _dark: '#0A0A0A',
            },
          },
          panel: {
            value: {
              _light: '#FFFFFF',
              _dark: '#131313',
            },
          },
          raised: {
            value: {
              _light: '#F4F4F5',
              _dark: '#1A1A1A',
            },
          },
        },
        bg: {
          DEFAULT: { value: '{colors.surface.canvas}' },
          subtle: { value: '{colors.surface.canvas}' },
          muted: { value: '{colors.surface.panel}' },
          emphasized: { value: '{colors.surface.raised}' },
          panel: { value: '{colors.surface.panel}' },
        },
        fg: {
          DEFAULT: {
            value: {
              _light: '{colors.gray.900}',
              _dark: '{colors.gray.100}',
            },
          },
          muted: {
            value: {
              _light: '{colors.gray.600}',
              _dark: '{colors.gray.400}',
            },
          },
          subtle: {
            value: {
              _light: '{colors.gray.500}',
              _dark: '{colors.gray.500}',
            },
          },
        },
        border: {
          DEFAULT: {
            value: {
              _light: 'rgba(0, 0, 0, 0.12)',
              _dark: 'rgba(255, 255, 255, 0.14)',
            },
          },
          muted: {
            value: {
              _light: 'rgba(0, 0, 0, 0.08)',
              _dark: 'rgba(255, 255, 255, 0.08)',
            },
          },
          emphasized: {
            value: {
              _light: 'rgba(0, 0, 0, 0.2)',
              _dark: 'rgba(255, 255, 255, 0.22)',
            },
          },
        },
        progress: {
          track: {
            value: {
              _light: '#E4E4E8',
              _dark: '#1A1A1A',
            },
          },
          fill: {
            value: {
              _light:
                'var(--app-progress-fill-light, color-mix(in srgb, #F07818 44%, #E4E4E8))',
              _dark:
                'var(--app-progress-fill-dark, color-mix(in srgb, #F07818 52%, #1A1A1A))',
            },
          },
        },
      },
    },
    recipes: {
      input: {
        base: {
          bg: 'bg.emphasized',
          borderColor: 'border',
          color: 'fg',
          _placeholder: { color: 'fg.subtle' },
          _focusVisible: {
            borderColor: 'brand.500',
            outlineColor: 'brand.500',
          },
        },
      },
    },
  },
})

export const system = createSystem(defaultConfig, appConfig)
