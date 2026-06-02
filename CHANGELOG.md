# Changelog

All notable changes to this project are documented here.

Release flow: update this file → commit → push → publish a GitHub release (AppImage is built by CI).

## Unreleased

### Fixed

- AppImage CI: install `libnotify4` and related GTK/WebKit runtime libs before `linuxdeploy` (fixes missing `libnotify.so.4`).

### Added

- GitHub Actions workflow `release-appimage.yml`: builds `TransmissionNET-<version>-x86_64.AppImage` on release publish or manual dispatch (Ubuntu 24.04, WebKitGTK 4.1).
- `packaging/appimage/`: build script, desktop entry, SVG icon, WebKit/GTK display hooks for Photino on Linux.

### Changed

- `.gitignore`: ignore `build/appimage/`, `dist/`, and `*.AppImage` artifacts.
