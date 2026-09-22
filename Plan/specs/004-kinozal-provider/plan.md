# Implementation Plan: Kinozal provider

## Approach

DLL-плагин поверх неизменного `ITorrentProvider`. Клиент держит cookies и mirror-fallback; UI (login-окно) — Avalonia.

## Steps (K0–K3)

1. K0 — Спецификация.
2. K1 — Client login/session/search/download + fixtures/tests.
3. K2 — Login window + MSBuild → `providers/`.
4. K3 — Settings `BaseUrl`; smoke.

## Affected layers / projects

- `src/TransmissonNET.Providers.Kinozal` (DLL → `providers/`).
- Справочно: `reference/torrent-providers.md`.

## Risks & mitigations

| Риск | Митигация |
|------|-----------|
| Разметка меняется | HTML-парсер за интерфейсом + fixtures |
| windows-1251 | встроенный кодек |