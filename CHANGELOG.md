# Changelog

All notable changes to this project are documented here.

Release flow: update this file → commit → push → publish a GitHub release (AppImage is built by CI).

## Unreleased

### Fixed

- AppImage CI: install `libnotify4` and related GTK/WebKit runtime libs before `linuxdeploy` (fixes missing `libnotify.so.4`).
- AppImage CI: use `APPIMAGE_VERSION` instead of `VERSION`; manual runs use SemVer `0.0.0-ci.<n>` so `dotnet publish` does not fail on invalid NuGet version strings.
- AppImage CI: set `DEPLOY_GTK_VERSION=3` for linuxdeploy gtk plugin (app binary lives under `opt/`, auto-detect fails).
- AppImage packaging: publish into `AppDir/usr/bin` with `Exec=TransmissonNET.App` so linuxdeploy can wire AppRun; split bundling and AppImage output into separate passes.
- AppImage CI: create the `.AppImage` in `build/appimage/` (linuxdeploy writes to CWD); `finalize` also checks repo root as fallback.
- AppImage CI: build with `appimagetool` into `dist/$OUTPUT_NAME` (explicit path); strip POSIX ACLs before packaging to avoid xattr noise.
- AppImage runtime: bundle WebKit helpers under `usr/lib/x86_64-linux-gnu/webkit2gtk-4.1`, patch `libwebkit` paths, set `LD_LIBRARY_PATH` in AppRun hook.
- AppImage runtime: custom `AppRun` `cd`s into mount dir (fixes `./usr` WebKit paths); default `GDK_BACKEND=x11` and `WEBKIT_DISABLE_SANDBOX=1` on Linux.
- AppImage runtime: WebKit helpers in `usr/lib/webkit2gtk-4.1` with paths relative to `usr/lib` (not cwd); build verifies no absolute `/usr/.../webkit2gtk` left in `libwebkit`.
- Desktop host: use `AppContext.BaseDirectory` for content root and `wwwroot` so the AppImage finds UI when launched from any working directory.

### Added

- GitHub Actions workflow `release-appimage.yml`: builds `TransmissionNET-<version>-x86_64.AppImage` on release publish or manual dispatch (Ubuntu 24.04, WebKitGTK 4.1).
- `packaging/appimage/`: build script, desktop entry, SVG icon, WebKit/GTK display hooks for Photino on Linux.

### Changed

- `.gitignore`: ignore `build/appimage/`, `dist/`, and `*.AppImage` artifacts.
