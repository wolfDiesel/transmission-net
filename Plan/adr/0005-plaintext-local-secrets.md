# 5. Plaintext-хранение локальных секретов (признанный долг)

**Date**: 2026-09-22

**Status**: Accepted

## Context

Пароль transmission-daemon и куки провайдеров (RuTracker/Kinozal/LostFilm) нужно
где-то хранить между запусками, чтобы не вводить их каждый раз.

## Decision

Храним их plaintext в `~/.config/TransmissonNET/` (`settings.json`,
`providers/<id>/cookies.json`). Это осознанное упрощение: не реализуем keyring
(libsecret) в текущей итерации.

## Consequences

- Пароль/куки читаются любым процессом текущего пользователя — риск осознан.
- Долг зафиксирован в `constitution.md` (раздел «Безопасность») и в спеках с
  приоритетом, а не замалчивается.
- Будущая миграция — на libsecret/keyring без смены контрактов `ISettingsStore`/куки.