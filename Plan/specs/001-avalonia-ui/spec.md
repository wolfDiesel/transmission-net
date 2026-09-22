# Feature Specification: Avalonia UI

**Status**: `Shipped`
**Источник**: `Plan/AVALONIA-PLAN.md` (ветка `feature/avalonia-ui`).

## Overview

Нативный Avalonia UI в отдельном исполняемом проекте `TransmissonNET.App.Avalonia`. ViewModels вызывают Application-хендлеры напрямую (без Kestrel/React). Photino/WebKit-путь сохраняется на `main` параллельно.

## User Scenarios & Testing

### US-1 Навигация по разделам
- **As a** пользователь, **I want** боковую навигацию с разделами и статус-баром, **so that** быстро переключаться между экранами.
- **GIVEN** запущено приложение **WHEN** выбран раздел **THEN** показан соответствующий экран (`ContentControl` + `ViewLocator`).

### US-2 Управление торрентами
- **As a** пользователь, **I want** таблицу торрентов с polling и контекстным меню (move/remove), **so that** управлять раздачами.
- **GIVEN** есть торренты **WHEN** открыт список **THEN** таблица показывает данные и периодически обновляется.

### US-3 Добавление и детализация
- **As a** пользователь, **I want** добавлять торренты, видеть детали и массово переименовывать, **so that** управлять контентом.
- **GIVEN** `.torrent`-файл **WHEN** добавлен **THEN** торрент появляется в списке.

## Requirements

- **FR-1** Shell: sidebar (Torrents/Search/Add/Settings), status bar, `ThemeService`.
- **FR-2** Общий Linux desktop `TransmissonNET.Desktop`: single instance, tray, обработка `.torrent` из CLI; Photino-проект также ссылается на него.
- **FR-3** Torrents: таблица, polling, context menu, move/remove.
- **FR-4** Settings: connection/daemon/interface.
- **FR-5** Add torrent, детали торрента, mass rename.
- **FR-6** i18n en/ru (+de/fr) через `LocalizationService`.
- **FR-7** Packaging: AppImage (`packaging/appimage/build-avalonia-appimage.sh`).

## Non-Goals

- Удаление/замена Photino+React на `main` (параллельная ветка).
- Поиск по трекерам (specs 002–004).

## Success Criteria

- [ ] `dotnet run --project src/TransmissonNET.App.Avalonia` открывает окно.
- [ ] ~100–200 МБ RAM в простое (замер `ps -o rss=` после 30 с простоя).
- [ ] `dotnet test` зелёные.
- [ ] AppImage собирается.

## Edge Cases & Error Handling

- EC-1 — Daemon недоступен → ошибка в UI, не крах.
- EC-2 — Локализация отсутствует для языка → fallback на en.