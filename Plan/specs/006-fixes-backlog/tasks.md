# Tasks: Backlog исправлений

## P0 (сначала)
- [ ] T1 — FR-4 RPC: один `HttpClient` + кеш `_sessionId` + 1 запрос на poll
- [ ] T2 — FR-5 `JsonSettingsStore`: tmp + `File.Move(overwrite:true)`, чтение бэкапа/default
- [ ] T3 — FR-6 шифрование пароля (keyring/0600) + маскирование DTO + HTTPS в `DaemonConnection`

## P1 (UI/Desktop/Providers)
- [ ] T4 — FR-1 `IProviderUiHost` + DI-конструкторы провайдеров + loader через `ActivatorUtilities`
- [ ] T5 — FR-2 трей/`wmctrl`/WebLogin: guard + изоляция за интерфейсом
- [ ] T6 — FR-3 VM без `AppServices`/`Application.Current`, под-VM, DI-фабрики окон

## P1 (остальные)
- [ ] T7 — FR-7 явные таймауты (RPC, `DesktopProcessRunner`, `HandlerInvoker`)
- [ ] T8 — FR-8 единый контракт ошибок (success/typed error)
- [ ] T9 — FR-11 лимиты парсера (глубина/элементы/размер)
- [ ] T10 — FR-12 тесты (RPC-ошибки, JSON, провайдеры, desktop, VM)

## P2
- [ ] T11 — FR-9 разбить `ExecuteTorrentActionHandler`
- [ ] T12 — FR-10 Domain без UI-настроек, приоритет `enum`