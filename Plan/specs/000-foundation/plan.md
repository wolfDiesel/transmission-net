# Implementation Plan: Foundation

## Approach

Слоистая архитектура по `constitution.md` §2. RPC-клиент скрыт за портом `ITransmissionClient` в Application; реализация в Infrastructure.

## Steps

1. Solution skeleton: `TransmissonNET.slnx` + class libraries Domain/Application/Infrastructure + тестовые проекты; ссылки по слоям.
2. Domain: `DaemonConnection`, `AppSettings`, `UiSettings`, `Torrent`, `TorrentStatus`.
3. Application ports: `ITransmissionClient` (`GetSessionAsync`, `GetTorrentsAsync`), `IRpcMethodNaming`.
4. Infrastructure: `TransmissionRpcClient` (POST JSON-RPC, заголовки), `RpcSessionState`, `RpcMethodNaming`, `TorrentMapper`, `TransmissionRpcException`.
5. `JsonSettingsStore` (Application порт `ISettingsStore`).
6. Use cases (scoped handlers) + регистрация в `Application/DependencyInjection.cs`.
7. REST endpoints (App) + DI wiring.

## Affected layers / projects

- `TransmissonNET.Domain`, `.Application`, `.Infrastructure`, `.App`.
- Тесты: `tests/TransmissonNET.Infrastructure.Tests`, `tests/TransmissonNET.Application.Tests`.

## Risks & mitigations

| Риск | Митигация |
|------|-----------|
| Имена RPC-методов (kebab/snake) | `RpcMethodNaming` + `reference/transmission-rpc.md` |
| 409 race при параллельных вызовах | кеш session-id на инстансе клиента |
| Plaintext пароль | alert в UI + `.gitignore` (долг фиксирован в `006-fixes-backlog` F6) |