# Implementation Plan: [FEATURE]

**Branch**: `NNN-slug` | **Date**: [DATE] | **Spec**: `Plan/specs/NNN-slug/spec.md`

## Summary

[Главное требование из спеки + выбранный технический подход из исследования.]

## Technical Context

**Language/Version**: .NET 8 / C# 12 (или NEEDS CLARIFICATION)
**Primary Dependencies**: Avalonia, CommunityToolkit.Mvvm, Microsoft.Extensions.DependencyInjection
**Storage**: `~/.config/TransmissonNET/` (`settings.json`, `providers/*`) или N/A
**Testing**: xUnit + Moq
**Target Platform**: Linux desktop
**Project Type**: desktop-app
**Performance Goals**: [например, цикл опроса ≤ N c, p95 < 200ms] или NEEDS CLARIFICATION
**Constraints**: [ограничения, например «оффлайн-способность»] или NEEDS CLARIFICATION
**Scale/Scope**: [масштаб] или NEEDS CLARIFICATION

## Constitution Check

*GATE: должно выполняться до начала работы; перепроверить после проектирования.*

- [ ] Слои и направление зависимостей не нарушены.
- [ ] Бизнес-логика — только в `*Handler` (Application), без HTTP/UI.
- [ ] Новые VM — constructor injection, без `AppServices`, без `Application.Current`.
- [ ] Секреты не попадают в git/логи.
- [ ] Acceptance-критерии — в GIVEN/WHEN/THEN.

## Project Structure

### Документация фичи

```text
Plan/specs/NNN-slug/
├── spec.md
├── plan.md
├── tasks.md
├── research.md    # (опц.) результаты исследования
└── data-model.md  # (опц.) модель данных
```

### Исходный код

```text
src/<проект>/
tests/<проект>.Tests/
```

**Structure Decision**: [какие проекты/файлы затрагиваются и почему.]

## Complexity Tracking

> Заполнять, только если Constitution Check дал нарушения, требующие обоснования.

| Нарушение | Зачем | Почему отвергнута более простая альтернатива |
|-----------|-------|----------------------------------------------|
| … | … | … |