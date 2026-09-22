# 2. Порты-адаптеры (слоистая архитектура)

**Date**: 2026-09-22

**Status**: Accepted

## Context

Нужно разделить бизнес-логику от инфраструктуры (HTTP/RPC, файлы настроек) и от UI,
чтобы логику можно было тестировать без живого daemon и заменить транспорт/UI без
переписывания use-case'ов.

## Decision

Принята слоистая структура с direction-правилом зависимостей:

```
Domain          → без зависимостей
Application     → Domain + Providers.Abstractions
Infrastructure  → Application (реализует порты I*)
App.Avalonia    → Application
Desktop         → GLib/GTK P/Invoke-хелперы
Providers.*     → только Providers.Abstractions
```

Приложение объявляет порты (`ITransmissionClient`, `ISettingsStore`, …) в
`Application/Abstractions/`, инфраструктура их реализует. Бизнес-логика живёт только
в `*Handler` (scoped, метод `HandleAsync`).

## Consequences

- Логику `*Handler` можно тестировать моками портов без сети/UI.
- Запрещены ссылочные циклы; Domain не тянет `HttpClient`/`System.Net`/JSON.
- Плата — больше проектов и дисциплина при добавлении нового порта.