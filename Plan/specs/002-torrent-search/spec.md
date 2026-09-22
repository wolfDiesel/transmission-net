# Feature Specification: Torrent Search (провайдеры)

**Status**: `Shipped`
**Источник**: `Plan/SEARCH-PLAN.md` (Stage B).

## Overview

Pluggable torrent-провайдеры как отдельные DLL. Вкладка Search в Avalonia UI. Первый провайдер — RuTracker. Поиск всех выбранных провайдеров выполняется параллельно.

## User Scenarios & Testing

### US-1 Поиск по выбранным провайдерам
- **As a** пользователь, **I want** искать по нескольким провайдерам параллельно, **so that** видеть результаты в одной таблице.
- **GIVEN** выбраны провайдеры и введён query **WHEN** запущен поиск **THEN** результаты всех провайдеров объединены в таблицу (Name/Source/Size/Link/Download).

### US-2 Авторизация провайдера
- **As a** пользователь, **I want** логиниться в провайдер (окно плагина), **so that** искать в закрытом трекере.
- **GIVEN** провайдер требует логин **WHEN** вызван `LoginAsync` **THEN** открывается окно входа; статус логина отображается на теге.

### US-3 Скачивание → Add preview
- **As a** пользователь, **I want** скачать `.torrent` из результата поиска и увидеть preview перед добавлением, **so that** осознанно добавить в daemon.
- **GIVEN** выбран результат **WHEN** вызван download **THEN** `.torrent`-байты открывают вкладку Add с превью; добавление — по явному подтверждению.

## Requirements

- **FR-1** Контракт `ITorrentProvider` (Id, DisplayName, IsLoginRequired, IsLoggedIn, Results, LoginAsync, LogoutAsync, SearchAsync, DownloadTorrentAsync, GetSettings/SetSettings) + `TorrentProviderSettings` + `TorrentSearchHit` (см. `reference/torrent-providers.md`).
- **FR-2** Loader/catalog: загрузка `providers/*.dll` (`TorrentProviderLoader`, `ITorrentProviderCatalog`).
- **FR-3** Поиск: fan-out через `Task.WhenAll` (`SearchAcrossProvidersHandler`).
- **FR-4** UI Search: теги провайдеров (× снять / + добавить), query + Search справа, Enter запускает поиск; DataGrid flatten.
- **FR-5** Download → Add preview → Add в daemon.
- **FR-6** Settings → Providers: per-plugin настройки (`GetSettings`/`SetSettings`, минимум `RequestTimeoutSeconds`).
- **FR-7** Login gate + `LogoutAsync` + индикатор логина на теге.
- **FR-8** RuTracker: login-окно, cookies/настройки в `~/.config/TransmissonNET/providers/rutracker/`, HTML windows-1251.
- **FR-9** `LoadErrors` каталога → StatusText + toast при открытии Search.

## Non-Goals

- Другие трекеры (кроме LostFilm/Kinozal — specs 003/004).
- Credentials провайдеров в app `settings.json`.
- Photino/React Search.
- Magnet / прямой torrent-add минуя Add.
- Пагинация / категории RuTracker.

## Success Criteria

- [ ] Контракт стабилен; плагины ссылаются только на `Providers.Abstractions`.
- [ ] `dotnet test` зелёные (loader/catalog/Fake tests).
- [ ] Ручной smoke: параллельный поиск → таблица; Download → Add preview → Add.

## Edge Cases & Error Handling

- EC-1 — Провайдер недоступен/падает при поиске → это не роняет другие провайдеры (`Task.WhenAll`).
- EC-2 — Ошибка загрузки DLL → зафиксирована в `LoadErrors`, показан toast.
- EC-3 — Не залогинен в трекер → login gate перед Search/Download.