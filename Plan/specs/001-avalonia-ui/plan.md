# Implementation Plan: Avalonia UI

## Approach

MVVM (CommunityToolkit.Mvvm). VM — scoped, окна — DI-фабрики. VM вызывают Application-хендлеры через `HandlerInvoker`. Общая Linux-часть выносится в `TransmissonNET.Desktop`.

## Steps (A0–A7)

1. A0 — ветка + spike: окно, список торрентов через `GetTorrentsHandler`.
2. A1 — Shell: sidebar, navigation, status bar, `ThemeService`.
3. A2 — `TransmissonNET.Desktop`; Photino + Avalonia на shared desktop.
4. A3 — Torrents: таблица, polling, context menu, move/remove.
5. A4 — Settings: connection, daemon, interface.
6. A5 — Add torrent, details, mass rename.
7. A6 — i18n en/ru (`Localization/*.json`).
8. A7 — AppImage-скрипт, `dotnet test`, RAM checklist.

## Affected layers / projects

- `src/TransmissonNET.App.Avalonia`, `src/TransmissonNET.Desktop`, `src/TransmissonNET.App` (Photino — ссылается на Desktop).
- Справочно: `reference/avalonia-ui.md`.

## Risks & mitigations

| Риск | Митигация |
|------|-----------|
| RAM выше цели | замер после 30 с простоя; без WebKit/Kestrel |
| Нарушение MVVM | `constitution.md` §4; под-VM вместо god-VM |
| Конфликт Photino/Avalonia | общий `Desktop`, UI-проекты разделены |