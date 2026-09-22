# Feature Specification: Search spinner + auto-navigation

**Status**: `Shipped`
**Источник**: `Plan/SEARCH-ADD-NAV-PLAN.md`.

## Overview

Две UI-фичи одной сессией: (1) спиннер поиска на теге провайдера вместо статичной точки логина; (2) автоматический переход на Torrents с выделением добавленного торрента после Add из поиска.

## User Scenarios & Testing

### US-1 Видимость прогресса поиска
- **As a** пользователь, **I want** видеть, какой провайдер ещё ищет, а какой завершил, **so that** понимать прогресс параллельного поиска.
- **GIVEN** запущен поиск по нескольким провайдерам **WHEN** один завершился/упал/отменён **THEN** его тег меняет состояние (спиннер → точка статуса логина).

### US-2 Переход после добавления
- **As a** пользователь, **I want** после успешного Add автоматически попадать на список торрентов, **so that** видеть добавленный торрент.
- **GIVEN** торрент добавлен из поиска **WHEN** Add завершился **THEN** происходит переход на Torrents, добавленный торрент выделен и видим (`ScrollIntoView`).

## Requirements

- **FR-1** Событие `SearchCompleted` в `ITorrentProvider` (`event EventHandler<ProviderSearchCompletedEventArgs>?`), вызывается в `finally` после `SearchAsync`; `Canceled=true` при `OperationCanceledException`. Сигнал — часть контракта, без знания провайдером UI.
- **FR-2** `ProviderSearchCompletedEventArgs` (ProviderId, Canceled, Error).
- **FR-3** В SearchViewModel: при старте поиска `IsSearching=true` для всех активных тегов; подписка на `SearchCompleted` → снять `IsSearching=false`.
- **FR-4** Верстка тега: `IsSearching` → маленький `ProgressBar IsIndeterminate`; иначе точка (зелёная/серая).
- **FR-5** Событие `TorrentAdded(int id, string name)` в `AddTorrentViewModel` после успешного `AddTorrentHandler`.
- **FR-6** MainWindowViewModel подписывается на `TorrentAdded` → `Navigate(AppPage.Torrents)` + `HighlightTorrent(id, name)`.
- **FR-7** `HighlightTorrent`: поиск по `Id`, fallback по `Name`; если нет в списке — refresh + повторный поиск. Хеш (`HashString`) в VM не доходит.
- **FR-8** `ScrollIntoView(TorrentsGrid.SelectedItem)` в code-behind `TorrentsView`.

## Non-Goals

- Локализация (спиннер без текста).
- Прямой magnet-add.
- Изменение навигационной модели (enum-переключатель сохраняется).

## Success Criteria

- [ ] Спиннер гаснет при завершении поиска (успех/ошибка/отмена).
- [ ] `ITorrentProvider` сигнализирует о завершении асинхронно, без знания UI.
- [ ] После Add — переход на Torrents, торрент выделен и виден.
- [ ] `dotnet test` зелёные.

## Edge Cases & Error Handling

- EC-1 — Провайдер отменён (`OperationCanceledException`) → `Canceled=true`, спиннер гаснет.
- EC-2 — Торрент ещё не в списке → повторный рефреш и поиск по id/name.