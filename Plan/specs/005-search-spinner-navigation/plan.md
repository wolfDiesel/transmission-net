# Implementation Plan: Search spinner + auto-navigation

## Approach

Состояние — только через observables/события, без статических локаторов. Контракт провайдера расширяется событием; UI реагирует через подписки.

## Steps

### Эпик 1 — спиннер
1. `ITorrentProvider.SearchCompleted` + `ProviderSearchCompletedEventArgs`.
2. Вызов события в `finally` после `SearchAsync` (RuTracker/Kinozal/LostFilm/Fake).
3. `SearchProviderTagViewModel.IsSearching`.
4. SearchViewModel: start → `tag.IsSearching=true`; подписка на `SearchCompleted` → `IsSearching=false`.
5. Вёрстка тега: спиннер vs точка.

### Эпик 2 — auto-navigation
6. `AddTorrentViewModel.TorrentAdded` (id, name) после успешного Add.
7. MainWindowViewModel: подписка → `Navigate(Torrents)` + `HighlightTorrent`.
8. `TorrentsViewModel.HighlightTorrent(id, name?)` (Id → Name fallback; refresh при отсутствии).
9. `TorrentsView.axaml.cs`: `ScrollIntoView` при смене выделения.

## Affected layers / projects

- `Providers.Abstractions` (контракт), `Providers.*` (сигнал), `App.Avalonia` (UI).

## Risks & mitigations

| Риск | Митигация |
|------|-----------|
| Событие не вызвано в некоторых ветках | `finally`-блок |
| Хеш не доходит до VM | поиск по Id, fallback Name (достаточно надёжно) |