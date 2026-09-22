# 3. Avalonia вместо Photino/React

**Date**: 2026-09-22

**Status**: Accepted

## Context

Изначальный UI строился на Photino + React (web-технологии в нативном окне). Это
тянуло двойной стек, усложняло пакетирование AppImage и расходилось с «только
Linux, нативно» позиционированием.

## Decision

Единственный живой UI — **Avalonia** (`src/TransmissonNET.App.Avalonia`).
Photino/React-хвосты помечены LEGACY и в новых фичах не развиваются.

## Consequences

- Один UI-стек, нативные контролы, проще паковать AppImage.
- Всё UI-состояние — в `ViewModel`-классах Avalonia-проекта, а не в React.
- LEGACY-справочники (`reference/web-ui.md`, `reference/photino-host.md`) остаются
  как архив, чтобы не терять контекст.