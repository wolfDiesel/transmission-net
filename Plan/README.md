# TransmissionNET — Spec-Driven Development (SDD)

Этот каталог — **единственный источник правды** по планированию TransmissionNET. Он организован по принципу Spec-Driven Development (SDD): спецификация («что») отделена от плана реализации («как») и списка задач.

## Структура

```
Plan/
  README.md            # этот файл — как устроен SDD-процесс
  constitution.md      # непреложные законы проекта (слои, именование, запреты, релиз)
  adr/                 # Architecture Decision Records — решения и их «почему»
  templates/           # шаблоны для новых спек
    spec-template.md       # «что»: user stories, FR, SC, GIVEN/WHEN/THEN
    plan-template.md       # «как»: подход, technical context, constitution check
    tasks-template.md      # задачи по фазам и user stories
    agent-file-template.md # шаблон инструкции для агента
  specs/               # по одной папке на фичу
    NNN-slug/
      spec.md          # требования, сценарии, GIVEN/WHEN/THEN, non-goals, success criteria
      plan.md          # архитектура/подход, шаги, риски
      tasks.md         # трекаемые задачи
  reference/           # справочные материалы (доменные знания, контракты, токены)
  scripts/             # операционные скрипты (create-new-feature.sh и др.)
```

## Как работать (workflow)

1. **Новая фича** — запусти `Plan/scripts/create-new-feature.sh <slug>`: создастся `specs/NNN-slug/` из шаблонов. Заполни `spec.md` (только «что», без реализации).
2. **Согласуй** `spec.md` — требования и acceptance-критерии должны быть однозначно проверяемыми.
3. Напиши `plan.md` (подход, шаги) и `tasks.md` (checklist).
4. Реализуй по задачам; перед закрытием — `dotnet test`.
5. **Изменение поведения** — сначала правь `spec.md`, затем код; никогда не расходи spec и код.

## Каталог спек

| # | Спека | Источник | Статус |
|---|-------|----------|--------|
| 000 | `specs/000-foundation/` | `MVP-PLAN.md` (backend-ядро) | Shipped |
| 001 | `specs/001-avalonia-ui/` | `AVALONIA-PLAN.md` | Shipped |
| 002 | `specs/002-torrent-search/` | `SEARCH-PLAN.md` | Shipped |
| 003 | `specs/003-lostfilm-provider/` | `LOSTFILM-PLAN.md` | Shipped |
| 004 | `specs/004-kinozal-provider/` | `KINOZAL-PLAN.md` | Shipped |
| 005 | `specs/005-search-spinner-navigation/` | `SEARCH-ADD-NAV-PLAN.md` | Shipped |
| 006 | `specs/006-fixes-backlog/` | `FIXES-PLAN.md` | Ready |

## Законы и архитектурные решения

Непреложные правила — в [`constitution.md`](constitution.md). Ключевые архитектурные
решения и их обоснование («почему») — в [`adr/`](adr/README.md). Доменные знания
(RPC, контракт провайдеров, тема) — в [`reference/`](reference/).