# Theme tokens (Lamborghini orange / black)

## Colors

| Token | Hex | Use |
|-------|-----|-----|
| brand.500 | `#FF7800` | primary buttons, активная навигация |
| brand.600 | `#DB6B02` | hover |
| gray.950 | `#0A0A0A` | фон приложения |
| gray.900 | `#111111` | sidebar |
| gray.800 | `#141414` | карточки, шапка таблицы |

## Chakra v3 (legacy sketch)

```ts
const system = createSystem(defaultConfig, {
  theme: {
    tokens: {
      colors: {
        brand: {
          500: { value: '#FF7800' },
          600: { value: '#DB6B02' },
        },
      },
    },
  },
});
```

`config.initialColorMode = 'dark'`.

## Components

- Sidebar: `bg="gray.900"`, активный пункт `color="brand.500"` + левая граница.
- Primary button: `colorPalette="orange"` или кастомный `brand`.
- Table: опционально zebra; шапка `gray.800`.

> Для Avalonia эти токены переносятся в `ThemeService`/`UiSettings` (паритет, см. `reference/avalonia-ui.md`).