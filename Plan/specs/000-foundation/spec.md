# Feature Specification: Foundation (ядро backend)

**Status**: `Shipped` (UI отдельно — `001-avalonia-ui`)
**Источник**: `Plan/MVP-PLAN.md`, этапы 1–4.

## Overview

Базовый backend TransmissionNET: доменные модели, Application-порты и use cases, RPC-клиент transmission-daemon и JSON-хранилище настроек. Linux-only, .NET 8. UI (Photino/React → Avalonia) вынесен из этой спеки.

## User Scenarios & Testing

### US-1 Просмотр списка торрентов
- **As a** пользователь, **I want** видеть список торрентов daemon с прогрессом/скоростями, **so that** контролировать загрузки.
- **GIVEN** настроено подключение к daemon и доступны торренты **WHEN** запрошен список **THEN** возвращаются торренты с полями `id/name/status/percentDone/rateDownload/rateUpload/eta/totalSize/error`.

### US-2 Подключение к daemon
- **As a** пользователь, **I want** проверить доступность daemon с введёнными учётными данными, **so that** убедиться, что подключение корректно.
- **GIVEN** host/port/username/password **WHEN** вызывается `session-get` **THEN** при успехе статус «connected», при неверном auth/недоступности — понятная ошибка.

### US-3 Сохранение настроек
- **As a** пользователь, **I want** сохранять и читать подключение, **so that** не вводить их повторно.
- **GIVEN** изменённые настройки **WHEN** сохраняются **THEN** они пишутся в `~/.config/TransmissonNET/settings.json` и читаются при старте.

## Requirements

- **FR-1** Domain-модели: `DaemonConnection` (host, port, rpcPath, username, password), `AppSettings` (daemon + ui), `UiSettings` (refreshIntervalSeconds, window size), `Torrent` (id, name, status, percentDone, rateDownload, rateUpload, eta, totalSize, error), enum `TorrentStatus`.
- **FR-2** RPC-транспорт: JSON-RPC `POST /transmission/rpc`; 409 → `X-Transmission-Session-Id` → один retry; HTTP Basic auth; имена методов kebab (rpc-version ≥ 17) или snake (< 17).
- **FR-3** Маппинг RPC-ответа в `Torrent` (см. `reference/transmission-rpc.md`).
- **FR-4** `JsonSettingsStore`: путь `~/.config/TransmissonNET/settings.json`; создание директории при первом save; defaults `127.0.0.1:9091`, interval 3s.
- **FR-5** Use cases: `GetSettingsHandler`, `SaveSettingsHandler` (валидация port 1–65535, interval ≥ 1), `TestConnectionHandler`, `GetTorrentsHandler`.
- **FR-6** REST/Minimal API (если включён Photino/Kestrel-путь): `GET /api/health`, `GET /api/settings`, `PUT /api/settings`, `POST /api/connection/test`, `GET /api/torrents`.
- **FR-7** Ошибки: не-success RPC → `TransmissionRpcException` → HTTP 502 `{ "error": ... }`; невалидные настройки → HTTP 400.

## Non-Goals

- UI (React/Photino) — вынесено в `001-avalonia-ui` (web-UI — legacy).
- Шифрование пароля в конфиге.
- Добавление/пауза/удаление торрентов, фильтры, детализация файлов.
- Поиск по трекерам (specs 002–004).
- Windows/macOS.

## Success Criteria

- [ ] `dotnet build` без ошибок.
- [ ] `dotnet test` (Infrastructure + Application) зелёные.
- [ ] 409-retry, Basic auth и маппинг полей покрыты тестами.
- [ ] Настройки сохраняются и читаются с диска.

## Edge Cases & Error Handling

- EC-1 — Неверный пароль/недоступный daemon → ошибка соединения, не крах приложения.
- EC-2 — `eta = -1` (неизвестно) и `error = 0` (нет ошибки) обрабатываются без падения.
- EC-3 — Первый контакт без session-id → 409 → retry с полученным id (ровно один retry).