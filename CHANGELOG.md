# Changelog

All notable changes to this project are documented here.

Release flow: update this file → commit → push → publish a GitHub release (AppImage is built by CI).

## Unreleased

### Added

- Linux `.torrent` integration: register default handler via user `.desktop` entry and `xdg-mime`; scan all `~/.local/share/applications/*.desktop` files and match TransmissionNET by parsed content (`Exec`, `Name`, `StartupWMClass`), update every match on register.
- Open `.torrent` from the file manager or CLI (`%f` / launch arg): pending path API, UI redirect to Add torrent with metainfo preview from file path.
- First-run prompt to become the default `.torrent` app (yes/no, stored in settings; not asked again after choice).
- Settings → Interface: button to register or refresh the `.torrent` association.
- Linux single-instance: second launch (e.g. opening a `.torrent`) forwards to the running app via Unix socket; UI polls for pending torrent path and reuses the same window (`wmctrl` focus when available).

### Fixed

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

- `.gitignore`: ignore `build/appimage/`, `dist/`, and `*.AppImage` artifacts.
