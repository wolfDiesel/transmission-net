# Architecture Decision Records (ADR)

Лог архитектурных решений TransmissionNET. Формат — Michael Nygard:

- **Status** — `Accepted` / `Proposed` / `Deprecated` / `Superseded`
- **Context** — почему решение назрело, какие силы действуют.
- **Decision** — что решили (императивно, без «возможно»).
- **Consequences** — что стало проще/сложнее, долги, компромиссы.

ADR фиксирует «почему», а не «что»: «что» живёт в `Plan/specs/`, непреложные
правила — в `Plan/constitution.md`. Новое архитектурное решение → новый файл
`NNNN-slug.md` и строка в таблице ниже.

## Каталог

| # | Решение | Статус |
|---|---------|--------|
| 0001 | Ведём логи архитектурных решений | Accepted |
| 0002 | Порты-адаптеры (слоистая архитектура) | Accepted |
| 0003 | Avalonia вместо Photino/React | Accepted |
| 0004 | Провайдеры torrent-поиска как standalone-плагины | Accepted |
| 0005 | Plaintext-хранение локальных секретов (признанный долг) | Accepted |
| 0006 | Транспорт RPC к transmission-daemon | Accepted |
| 0007 | Композиция UI (MVVM, service locator, enum-навигация) | Accepted |