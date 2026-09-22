# Torrent providers reference

## Contract (`TransmissonNET.Providers.Abstractions`)

- `ITorrentProvider` — `Id`, `DisplayName`, `IsLoginRequired`, `IsLoggedIn`, `Results`, `KnownMirrors`, `LoginAsync`, `LogoutAsync`, `SearchAsync`, `DownloadTorrentAsync`, `GetSettings`, `SetSettings`.
- `Results` — `ObservableCollection<TorrentSearchHit>` (источник правды; хост flatten'ит для DataGrid).
- `KnownMirrors` — известные зеркала для chips в Settings; `BaseUrl` — свободный кастомный mirror.
- `TorrentProviderSettings` — `RequestTimeoutSeconds`; опционально `BaseUrl`, `PreferredQuality`, `MaxSeriesExpand` (LostFilm).
- `TorrentSearchHit` — `record (string Id, string Title, long? SizeBytes, string? DetailUrl)`.
- Поиск fan-out: `SearchAcrossProvidersHandler` через `Task.WhenAll`.

Плагины ссылаются только на Abstractions (+ свои зависимости). Credentials остаются внутри плагина.

## Host

- Загрузка `providers/*.dll` через `TorrentProviderLoader`.
- Search: теги + query; login gate; Download → Add preview.
- Settings → Providers tab: per-plugin `GetSettings`/`SetSettings`.
- `LoadErrors`: `StatusText` + toast при открытии Search.
- Debug: `TransmissonNET.Providers.Dummy` также копируется в `providers/` для multi-provider UX.

## RuTracker

`TransmissonNET.Providers.RuTracker` — login-окно, cookies/settings в `~/.config/TransmissonNET/providers/rutracker/`, HTML windows-1251. См. `specs/002-torrent-search/`.

## LostFilm

`TransmissonNET.Providers.LostFilm` — mirror `lostfilm.download` + fallbacks; `lf_session` auth (login или вставка cookie); поиск `/search/` → `/seasons`-эпизоды; download через `v_search.php`. См. `specs/003-lostfilm-provider/`.

## Kinozal

`TransmissonNET.Providers.Kinozal` — `kinozal.me` + fallbacks; windows-1251; `takelogin.php` / `browse.php` / `download.php`. См. `specs/004-kinozal-provider/`.