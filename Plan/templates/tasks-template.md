# Tasks: [FEATURE NAME]

**Input**: документы из `Plan/specs/NNN-slug/` (`spec.md`, `plan.md`).
**Organization**: задачи группируются по user story; каждая — независимо реализуема и тестируема.

## Формат: `[ID] [P?] [USx] Описание`

- `[P]` — можно запускать параллельно (разные файлы, без зависимостей).
- `[USx]` — к какой user story относится задача.
- В описаниях — точные пути к файлам.

## Phase 1: Setup

- [ ] T001 Создать структуру проекта по `plan.md`.
- [ ] T002 [P] Настроить линт/формат.

## Phase 2: Foundational (блокирующие предусловия)

**⚠️ КРИТИЧНО**: ни одну user story нельзя начинать, пока эта фаза не завершена.

- [ ] T0xx [P] …
- [ ] T0xx …

**Checkpoint**: основание готово — user stories можно делать параллельно.

## Phase 3: User Story 1 (P1) 🎯 MVP

**Goal**: [что даёт story]
**Independent Test**: [как проверить отдельно]

- [ ] T0xx [US1] …
- [ ] T0xx [US1] …

## Phase N: Polish & Cross-Cutting Concerns

- [ ] T0xx [P] Обновить документацию.
- [ ] T0xx Прогнать `dotnet test`.

## Dependencies & Execution Order

- Setup → Foundational → User Stories (P1 → P2 → P3) → Polish.
- Внутри story: тесты (если нужны) пишутся первыми и должны падать до реализации.
- Модели раньше сервисов, сервисы раньше endpoint-ов.

## Execution Strategy

1. Foundation готов → реализуй US1 → проверь независимо → это MVP.
2. По одной story; каждая добавляет ценность, не ломая предыдущие.