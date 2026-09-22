# 6. Транспорт RPC к transmission-daemon

**Date**: 2026-09-22

**Status**: Accepted

## Context

Нужен надёжный вызов JSON-RPC transmission-daemon с учётом его особенностей
(CSRF-токен сессии, различия в именах методов по версии).

## Decision

- Транспорт — `Infrastructure/Rpc/TransmissionRpcClient.cs`.
- HTTP-only (`http://`), HTTPS не поддержан.
- `409 Conflict` + заголовок `X-Transmission-Session-Id` → ровно один retry.
- HTTP Basic auth.
- Имена методов: при `rpc-version >= 17` — kebab-case, иначе snake_case.

## Consequences

- Устойчивость к CSRF-сессии без ручного управления токеном.
- Нет шифрования транспорта (пароль идёт открыто) — согласуется с ADR-0005.
- `_sessionId` кешируется внутри вызова, а не между вызовами (клиент transient) —
  каждый цикл это session-get + speed-get + torrent-get.