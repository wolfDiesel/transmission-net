# Implementation Plan: LostFilm provider

## Approach

DLL-плагин поверх неизменного `ITorrentProvider`. Клиент держит сессию и mirror-fallback; UI (login-окно) — Avalonia.

## Steps (L0–L5)

1. L0 — Спецификация + ссылка в каталоге провайдеров.
2. L1 — Client: сессия, mirrors, login + cookie.
3. L2 — Search HTML + expand эпизодов → Results; tests.
4. L3 — Download + `PreferredQuality`; tests.
5. L4 — Login Avalonia window; MSBuild → `providers/`.
6. L5 — Settings (BaseUrl, PreferredQuality, MaxSeriesExpand); smoke.

## Affected layers / projects

- `src/TransmissonNET.Providers.LostFilm` (DLL → `providers/`).
- Справочно: `reference/torrent-providers.md`.

## Risks & mitigations

| Риск | Митигация |
|------|-----------|
| Зеркало меняет разметку | HTML-парсер за интерфейсом + fixtures |
| Cookie-сессия протухает | `LogoutAsync` + повторный login |