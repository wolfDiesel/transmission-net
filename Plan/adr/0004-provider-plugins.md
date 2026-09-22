# 4. Провайдеры torrent-поиска как standalone-плагины

**Date**: 2026-09-22

**Status**: Accepted

## Context

Поиск расширяется провайдерами (RuTracker, LostFilm, Kinozal). Хочется добавлять
провайдеры, не трогая ядро, и грузить их из отдельной папки `providers/`.

## Decision

Каждый провайдер — отдельный проект (`TransmissonNET.Providers.*`), собирается в
DLL рядом с приложением и грузится `TorrentProviderLoader` через public
parameterless constructor. Провайдеры не участвуют в DI ядра; UI (login-окна)
выделяется наружу через `IProviderUiHost`, чтобы провайдер не зависел от Avalonia.

## Consequences

- Провайдеры подключаемы/отключаемы без пересборки ядра.
- Провайдер не имеет доступа к DI-контейнеру — часть удобств теряется, но контракт
  остаётся простым и явным (`ITorrentProvider`).
- Runtime-изоляция плагинов (AppDomain/process) не используется — провайдеры
  доверенны.