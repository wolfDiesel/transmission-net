# Feature Specification: LostFilm provider

**Status**: `Shipped`
**Источник**: `Plan/LOSTFILM-PLAN.md` (Stage L).

## Overview

Pluggable torrent-провайдер DLL для LostFilm (зеркало `lostfilm.download` + fallback). Хост и контракт `ITorrentProvider` не меняются.

## User Scenarios & Testing

### US-1 Поиск сериала и эпизодов
- **As a** пользователь, **I want** искать по LostFilm и видеть эпизоды, **so that** скачивать нужные серии.
- **GIVEN** провайдер LostFilm активен и выполнен login **WHEN** запущен поиск **THEN** результаты содержат одну строку на эпизод.

### US-2 Вход по cookie
- **As a** пользователь, **I want** войти либо по email/password, либо вставкой cookie `lf_session`, **so that** получить доступ к закрытому контенту.
- **GIVEN** credentials **WHEN** выполнен login **THEN** сессия установлена; `LogoutAsync` её очищает.

### US-3 Download с выбором качества
- **As a** пользователь, **I want** качать `.torrent` с выбранным качеством (`PreferredQuality`), **so that** получать нужный рип.
- **GIVEN** задано `PreferredQuality` (1080/HD/SD/MP4) **WHEN** вызван download **THEN** `.torrent`-байты открывают Add preview.

## Requirements

- **FR-1** База `https://www.lostfilm.download/` + fallback-зеркала.
- **FR-2** Auth: email/password → cookie `lf_session`; fallback — вставка cookie в login-окне; `LogoutAsync` чистит сессию.
- **FR-3** Results: одна строка на эпизод; качество из `PreferredQuality`.
- **FR-4** Download: `v_search.php?c=&s=&e=` → страница качеств → `.torrent` bytes → Add preview.
- **FR-5** Данные в `~/.config/TransmissonNET/providers/lostfilm/`.
- **FR-6** Settings: `BaseUrl`, `PreferredQuality`, `MaxSeriesExpand` (+ обязательный `RequestTimeoutSeconds`).

## Non-Goals

- RSS-only, фильмы, избранное, proxy UI, Magnet.

## Success Criteria

- [ ] Login / cookie / Logout работают.
- [ ] Поиск → эпизоды в таблице.
- [ ] Download → Add preview.
- [ ] Настройки (`BaseUrl`, `PreferredQuality`, `MaxSeriesExpand`) сохраняются и применяются.
- [ ] `dotnet test` зелёные.

## Edge Cases & Error Handling

- EC-1 — Основное зеркало недоступно → fallback-зеркала.
- EC-2 — Неверный cookie → ошибка логина с понятным сообщением.