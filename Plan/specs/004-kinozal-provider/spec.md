# Feature Specification: Kinozal provider

**Status**: `Shipped`
**Источник**: `Plan/KINOZAL-PLAN.md` (Stage K).

## Overview

Pluggable DLL-провайдер для [kinozal.me](https://kinozal.me/) (зеркала + fallback). Контракт `ITorrentProvider` не меняется.

## User Scenarios & Testing

### US-1 Поиск по Kinozal
- **As a** пользователь, **I want** искать по Kinozal, **so that** видеть раздачи трекера в общей таблице поиска.
- **GIVEN** провайдер активен **WHEN** выполнен поиск **THEN** строки `tr` с `td.bt`/`td.nam` превращаются в `TorrentSearchHit`.

### US-2 Вход
- **As a** пользователь, **I want** логиниться через `takelogin.php` (windows-1251), **so that** получить cookies `uid`/`pass`.
- **GIVEN** username/password **WHEN** выполнен login **THEN** сессия установлена; `LogoutAsync` чистит её.

### US-3 Download
- **As a** пользователь, **I want** качать `.torrent` по `download.php?id={hitId}`, **so that** открывать Add preview.
- **GIVEN** выбран результат **WHEN** вызван download **THEN** `.torrent`-байты открывают Add preview.

## Requirements

- **FR-1** База `https://kinozal.me/` + fallback `kinozal.tv`, `kinozal.guru`.
- **FR-2** Auth: username/password → `takelogin.php` (windows-1251); cookies `uid`/`pass`; `LogoutAsync` чистит сессию.
- **FR-3** Search: `browse.php?s=…&c=0&g=0&v=0&d=0&w=0&t=0&f=0` → строки `tr` с `td.bt`/`td.nam`.
- **FR-4** Download: `download.php?id={hitId}` → `.torrent` bytes → Add preview.
- **FR-5** Данные в `~/.config/TransmissonNET/providers/kinozal/`.
- **FR-6** Settings: `RequestTimeoutSeconds`, `BaseUrl`.

## Non-Goals

- Magnet-обход дневного лимита, категории, freeleech-фильтр.

## Success Criteria

- [ ] Login / Logout работают.
- [ ] Поиск → таблица.
- [ ] Download → Add preview.
- [ ] Settings (`BaseUrl`/timeout) сохраняются.
- [ ] `dotnet test` зелёные.

## Edge Cases & Error Handling

- EC-1 — Основной домен недоступен → fallback-зеркала.
- EC-2 — Неверный логин → ошибка с понятным сообщением.