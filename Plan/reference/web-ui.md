# Web UI (React + Chakra) reference — LEGACY

> Устарело: основной UI переведён на Avalonia (см. `specs/001-avalonia-ui/` и `reference/avalonia-ui.md`).
> Оставлено как историческая справка по Photino/React-подходу на ветке `main`.

## Directory layout

```
web/transmission-ui/src/
  theme/          # Chakra system, tokens
  layout/         # AppShell (sidebar + header + Outlet)
  pages/          # TorrentsPage, SettingsPage
  api/            # fetch-обёртки для /api/*
  hooks/          # useTorrents, useSettings
  App.tsx         # Router
```

## Routes (MVP)

| Path | Page |
|------|------|
| `/` | Torrents |
| `/settings` | Daemon connection |

Новая фича = route + `pages/`-файл + ссылка в `AppShell`.

## API client

- База: относительный `/api` (same origin с Photino/Kestrel).
- Dev: Vite `server.proxy['/api']` → backend-порт.
- Ошибки 502 → user-visible сообщение из `{ error: string }`.

## Polling

`useTorrents(intervalSeconds)`: `useEffect`+`setInterval` для auto-refresh; `refreshNow()` для ручного; `lastUpdated` в шапке/подвале.

## Settings page

Поля: host, port, username, password, refresh interval. Кнопки: Test (`POST /api/connection/test`), Save (`PUT /api/settings`). Alert: пароль хранится plaintext.