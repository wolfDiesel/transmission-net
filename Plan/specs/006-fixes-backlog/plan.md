# Implementation Plan: Backlog исправлений

## Approach

Безопасные правки инфраструктуры первыми, крупные MVVM/UI-рефакторинги — позже. Каждый пункт — отдельный коммит/PR с зелёным `dotnet test`.

## Порядок выполнения

1. **FR-4, FR-5, FR-6** (P0) — быстрые и безопасные правки инфраструктуры/security.
2. **FR-1, FR-2, FR-3** — UI/Desktop/Providers.
3. **FR-7, FR-8, FR-11, FR-12** (P1).
4. **FR-9, FR-10** (P2, чистый долг).

## Принятые решения

### FR-1 — провайдеры, полный DI
- `IProviderUiHost` в `Providers.Abstractions` (без Avalonia): `LoginAsync(preferredUrl, dataDirectory, ct)` + маршалинг через `SyncContext`.
- Login-окна переносятся в `App.Avalonia`; плагин через `IProviderUiHost` запрашивает результат.
- Плагины: убрать `Dispatcher.UIThread`, `Window`, `Application.Current`, `*WebLogin`; ctor `(TorrentProviderSettings, IProviderUiHost, IProviderSessionStore)`.
- `TorrentProviderLoader`: `IServiceProvider` + `ActivatorUtilities.CreateInstance`.
- Хранение куки/сессии: до FR-6 не трогаем (только за фасадом `IProviderSessionStore`).

### FR-3 — MVVM, deep + headless
- `AppServices` убрать из VM; оставить как scope-фабрику для `HandlerInvoker`.
- VM scoped; окна/диалоги — DI-фабрики (`IWindowFactory`).
- `TorrentsViewModel` → под-VM (таблица/статус/фильтры).
- `NavigationService` не трогаем.
- Новый тест-проект `tests/TransmissonNET.App.Avalonia.Tests` (`Avalonia.Headless`).

## Affected layers / projects

- Infrastructure (RPC, Settings), Desktop, Providers, Application (handlers/contracts), App.Avalonia (VM/DI), тестовые проекты.

## Risks & mitigations

| Риск | Митигация |
|------|-----------|
| Рефакторинг DI провайдеров ломает поиск | поэтапно + test fixtures за интерфейсом |
| MVVM deep-рефакторинг раздувает диф | поэтапно, VM-тесты |
| Security (FR-6) зависит от окружения (keyring) | fallback на ключ с правами 0600 |