# 7. Композиция UI (MVVM, service locator, enum-навигация)

**Date**: 2026-09-22

**Status**: Accepted

## Context

Avalonia-приложение нужно связать с Application-слоем. Нужен способ навигации между
страницами (торренты / поиск / добавление / настройки) и доступ к сервисам.

## Decision

- MVVM через CommunityToolkit.Mvvm (source generators): VM — `internal`, наследуют
  `ViewModelBase`, `[ObservableProperty]`/`[RelayCommand]`.
- VM — синглтоны; страницы переключаются `NavigationService` по enum `AppPage`
  (без back/forward).
- Доступ к сервисам — статический `AppServices` (service locator); **новый** код
  должен использовать constructor injection, service locator — только в легаси.

## Consequences

- Простая навигация и сквозное состояние (таблица торрентов живёт, пока открыто окно).
- Накопленный технический долг: service locator в старом коде, нельзя
  back/forward по истории навигации.
- Направление для нового кода закреплено в `constitution.md`: constructor injection.