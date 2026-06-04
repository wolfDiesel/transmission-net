# Changelog

All notable changes to this project are documented here.

Release flow: update this file → commit → push → publish a GitHub release (AppImage is built by CI).

## Unreleased

### Added

- `README.md`: project overview for `transmission-daemon`, features, mass rename, build and AppImage notes.
- UI i18n: English and Russian locale files (`web/transmission-ui/src/i18n/locales/`), live language switch in Settings → Interface; `ui.language` persisted in app settings (`en` / `ru`).

### Added

- Linux system tray via libayatana-appindicator (GTK menu Show/Quit, close-to-tray, settings in Interface).
- Brand icon: orange wolf on transparent background in sidebar, favicon, tray, AppImage/desktop; `sync-brand-icons.sh` syncs SVG/PNG into UI and `wwwroot` on build.
- `GET /api/desktop/capabilities` and tray options in settings (`trayEnabled`, `closeToTray`, `minimizeToTray`).
- Linux `.torrent` integration: register default handler via user `.desktop` entry and `xdg-mime`; scan all `~/.local/share/applications/*.desktop` files and match TransmissionNET by parsed content (`Exec`, `Name`, `StartupWMClass`), update every match on register.
- Open `.torrent` from the file manager or CLI (`%f` / launch arg): pending path API, UI redirect to Add torrent with metainfo preview from file path.
- First-run prompt to become the default `.torrent` app (yes/no, stored in settings; not asked again after choice).
- Settings → Interface: button to register or refresh the `.torrent` association.
- Linux single-instance: second launch (e.g. opening a `.torrent`) forwards to the running app via Unix socket; UI polls for pending torrent path and reuses the same window (`wmctrl` focus when available).

### Fixed

- Linux single-instance: remove stale `transmission-net.sock` when the previous process died without cleanup.
- Linux tray: resolve `g_object_unref` from libgobject (fixes crash on Quit from tray menu on Fedora).
- Linux `.torrent` registration: write valid `.desktop` entries (no leading spaces), resolve stable AppImage path for `Exec`, set default handler via `gio mime` / `xdg-mime` with exit-code checks and UI error when the system keeps another app.
- Linux `.torrent` registration: fix `xdg-mime default` argument order; always register `transmission-net.desktop` and skip AppImage Manager `appimagemanager-*.desktop` stubs.
- Linux `.torrent` registration: write `.desktop` and `mimeapps.list` without UTF-8 BOM (fixes gio “could not load handler”); validate desktop entry, install icon, fallback to `mimeapps.list`; bundle `transmission-net.svg` in AppImage.
- Linux `.torrent` registration: update all matching `.desktop` files (including AppImage Manager `appimagemanager-*` shortcuts) and try each candidate when setting the default handler.
- AppImage CI: install `libnotify4` and related GTK/WebKit runtime libs before `linuxdeploy` (fixes missing `libnotify.so.4`).
- AppImage CI: use `APPIMAGE_VERSION` instead of `VERSION`; manual runs use SemVer `0.0.0-ci.<n>` so `dotnet publish` does not fail on invalid NuGet version strings.
- AppImage CI: set `DEPLOY_GTK_VERSION=3` for linuxdeploy gtk plugin (app binary lives under `opt/`, auto-detect fails).
- AppImage packaging: publish into `AppDir/usr/bin` with `Exec=TransmissonNET.App` so linuxdeploy can wire AppRun; split bundling and AppImage output into separate passes.
- AppImage CI: create the `.AppImage` in `build/appimage/` (linuxdeploy writes to CWD); `finalize` also checks repo root as fallback.
- AppImage CI: build with `appimagetool` into `dist/$OUTPUT_NAME` (explicit path); strip POSIX ACLs before packaging to avoid xattr noise.
- AppImage runtime: bundle WebKit helpers under `usr/lib/x86_64-linux-gnu/webkit2gtk-4.1`, patch `libwebkit` paths, set `LD_LIBRARY_PATH` in AppRun hook.
- AppImage runtime: custom `AppRun` `cd`s into mount dir (fixes `./usr` WebKit paths); default `GDK_BACKEND=x11` and `WEBKIT_DISABLE_SANDBOX=1` on Linux.
- AppImage runtime: use **system** WebKitGTK 4.1 and **system** GTK 3 (not bundled); fixes gray window / EGL mismatch on Fedora. Runtime: `dnf install webkit2gtk4.1` if missing.
- AppImage runtime: `LD_LIBRARY_PATH` only `usr/bin` (avoids Ubuntu `libmount` vs Fedora `libgio` symbol clash).
- AppImage runtime: drop default `WEBKIT_DISABLE_COMPOSITING_MODE` and `LIBGL_ALWAYS_SOFTWARE` so CSS hover/accent styles repaint in WebKit.
- UI: Chakra `_hover` no longer gated on `@media (hover: hover)` (fixes missing hovers in Photino/WebKitGTK); table row accent uses `--app-row-hover-bg` CSS variables.
- Desktop host: use `AppContext.BaseDirectory` for content root and `wwwroot` so the AppImage finds UI when launched from any working directory.

### Added

- GitHub Actions workflow `release-appimage.yml`: builds `TransmissionNET-<version>-x86_64.AppImage` on release publish or manual dispatch (Ubuntu 24.04, WebKitGTK 4.1).
- `packaging/appimage/`: build script, desktop entry, SVG icon, WebKit/GTK display hooks for Photino on Linux.

### Changed

- UI strings moved from inline text and `modeHelp.ts` into locale files; torrent table columns and context menu labels follow the active language.

### Changed

- `.gitignore`: ignore `build/appimage/`, `dist/`, and `*.AppImage` artifacts.
