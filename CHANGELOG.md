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

### Added

- GitHub Actions workflow `release-appimage.yml`: builds `TransmissionNET-<version>-x86_64.AppImage` on release publish or manual dispatch (Ubuntu 24.04, WebKitGTK 4.1).
- `packaging/appimage/`: build script, desktop entry, SVG icon, WebKit/GTK display hooks for Photino on Linux.

### Changed

- `.gitignore`: ignore `build/appimage/`, `dist/`, and `*.AppImage` artifacts.
