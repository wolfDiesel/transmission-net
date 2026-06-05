# Changelog

All notable changes to this project are documented here.

Release flow: update this file → commit → push → publish a GitHub release (AppImage is built by CI).

## Unreleased

### Changed

- **Desktop UI:** replace Photino/WebKit + React with native **Avalonia** (`TransmissonNET.App.Avalonia`); published binary remains `TransmissonNET.App`.
- **Removed:** `web/transmission-ui`, embedded Kestrel REST API, Photino host, WebKitGTK AppImage packaging, and CI Node.js build step.
- Linux desktop helpers moved to `TransmissonNET.Desktop` (tray, single-instance, `.torrent` argv).
- AppImage CI and `build-appimage.sh` target Avalonia only (GTK3 + Ayatana tray; no WebKit runtime).
- `sync-brand-icons.sh` copies brand assets into Avalonia `Assets/` only.

### Added

- UI i18n: German (`de`) and French (`fr`) locale catalogs; language picker in Settings → Interface.
- Torrent details → Files: three columns (name, priority icon, progress bar); file progress polls only while the Files tab is open.
- Per-file download priority in the file tree context menu (High / Normal / Low via Transmission `torrent-set`).
- Avalonia UI: torrent table with polling, settings, add torrent, details, mass rename, system tray, i18n (`en` / `ru` / `de` / `fr`), torrent name filter with wildcards.
- Avalonia `.torrent` launch: pending-path coordinator, Add torrent preview from file path, first-run association prompt, settings register button.
- `TransmissonNET.Desktop` shared library for Linux tray, single-instance socket, and CLI torrent path parsing.
- Tests for pending launch, MIME association handlers, `InspectTorrentMetainfoFromPath`, desktop message parsing, and torrent file priority RPC.
- Linux system tray via libayatana-appindicator; tray options in settings (`trayEnabled`, `closeToTray`, `minimizeToTray`).
- Linux `.torrent` integration: user `.desktop` entry + `xdg-mime`; match/update all TransmissionNET shortcuts under `~/.local/share/applications/`.
- Linux single-instance: second launch forwards path via Unix socket; running app focuses window and opens Add torrent.
- GitHub Actions `release-appimage.yml` and `packaging/appimage/` for AppImage builds.
- `README.md`: project overview, features, mass rename, build and AppImage notes.

### Fixed

- Torrent details window: stop file poll timer, unsubscribe localization handler, and clear view model on close to avoid leaks.
- Avalonia torrent table: stable row order on refresh/filter; progress bar binding; status bar speeds from torrent list; byte size units (GB vs TB).
- Linux single-instance: remove stale `transmission-net.sock` when the previous process died without cleanup.
- Linux tray: resolve `g_object_unref` from libgobject (fixes crash on Quit from tray menu on Fedora).
- Linux `.torrent` registration: valid `.desktop` entries, no UTF-8 BOM, stable AppImage `Exec`, `xdg-mime` argument order, skip AppImage Manager stubs, update all matching shortcuts.
- AppImage CI: `APPIMAGE_VERSION`, SemVer for manual runs, `DEPLOY_GTK_VERSION=3`, publish layout under `AppDir/usr/bin`, explicit output paths, ACL strip before packaging.
