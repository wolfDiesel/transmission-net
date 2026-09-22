# Implementation Plan: Torrent Search

## Approach

Плагинная архитектура: контракт в `Providers.Abstractions`, загрузка в Infrastructure, оркестрация в Application, UI в Avalonia. Credentials/сессия — внутри каждой DLL.

## Steps (B0–B6)

1. B0 — Abstractions (`ITorrentProvider` и др.) + спецификация.
2. B1 — `TorrentProviderLoader` + catalog + Fake-провайдер для тестов.
3. B2 — RuTracker login/search/download + fixtures.
4. B3 — Avalonia Search shell + i18n.
5. B4 — Wire search + Download→Add + login gate.
6. B5 — MSBuild копирование `providers/`, полировка состояний, `dotnet test`.
7. B6 — toast `LoadErrors`; индикатор логина на тегах; `LogoutAsync` + UI.

## Affected layers / projects

- `TransmissonNET.Providers.Abstractions`, `.Infrastructure` (loader/catalog), `.Application` (catalog + `SearchAcrossProvidersHandler`), `.Providers.RuTracker`, `.App.Avalonia`.
- Справочно: `reference/torrent-providers.md`.

## Risks & mitigations

| Риск | Митигация |
|------|-----------|
| HTML-парсинг хрупок (windows-1251) | встроенный кодек; fixtures для парсера |
| Провайдер тянет Avalonia/Dispatcher | `constitution.md` §4; выделение UI-хоста (долг F1) |
| Параллельный поиск — гонки | `Task.WhenAll` с изоляцией per-provider |