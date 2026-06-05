# TransmissionNET

Desktop client for [Transmission](https://transmissionbt.com/) (`transmission-daemon`). It talks to the daemon over the RPC API and ships a native **Avalonia** UI on Linux.

## Requirements

- A running `transmission-daemon` with RPC enabled (host, port, and credentials configured in the app).
- **Linux** for the desktop build (GTK 3, optional Ayatana AppIndicator for the system tray).
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) when building from source.

## Features

- **Torrent list** — status, progress, speeds, peers, name filter with wildcards, and per-torrent actions aligned with Transmission RPC.
- **Add torrents** — magnet links, `.torrent` files, and metainfo preview before adding.
- **Daemon connection** — RPC URL, authentication, and connection health in Settings.
- **Session settings** — edit daemon session options exposed by Transmission (where supported by the RPC layer).
- **English / Russian UI** — switch language in Settings → Interface (applied immediately; saved with app settings).
- **Torrent details** — file tree, priorities, labels, and per-file operations.
- **Mass rename** — batch-rename files inside a torrent from the file tree (see below).
- **Linux desktop integration**
  - Single instance: opening a second copy or a `.torrent` file forwards to the running window.
  - Optional system tray (Show / Quit, close-to-tray, minimize-to-tray) via `libayatana-appindicator3`.
  - Register as the default handler for `.torrent` files (user-level `.desktop` + MIME).
- **AppImage** — prebuilt x86_64 images attached to [GitHub releases](https://github.com/wolfDiesel/transmission-net/releases) (built on Ubuntu 24.04).

## Mass rename

Mass rename is aimed at cleaning up multi-file torrents (episodes, releases, music tracks) without leaving the client.

1. Open a torrent’s **Files** tab, right-click a folder (or the torrent root), and choose **Mass rename…**.
2. Pick a **scope** (that folder and its files). Only **file names** change; directory entries in the torrent layout are not renamed.
3. Choose a mode, tune options, and review the **preview** (changed segments are highlighted). Up to 200 rows are shown in the panel; the full plan is validated before apply.
4. **Apply** sends a single batch to the daemon (`torrent-rename-path` per file, ordered by path depth).

| Mode | Purpose |
|------|---------|
| **Regex** | Pattern on the full filename (extension included). Replacement supports `$1`, `$2`, …; flags `i` / `g`. |
| **Find/Replace** | Literal find/replace; optional case sensitivity and “stem only” (rule on name without extension). |
| **Prefix/Suffix** | Prepend and/or append text to the current name. |
| **Numbering** | New names from a template with `{n}` / `{n:02}`; **Start** and **Step** control the counter. |
| **Template** | Full new names from `{n}`, `{name}`, `{ext}`, `{basename}`, `{path}`; counter fields are shared with Numbering. |

Sorting for `{n}` can follow full path or file name. Validation catches collisions, empty results, and invalid regex before anything hits the daemon.

## Run from source

```bash
git clone https://github.com/wolfDiesel/transmission-net.git
cd transmission-net
dotnet run --project src/TransmissonNET.App.Avalonia/TransmissonNET.App.Avalonia.csproj
```

On **Fedora** (example runtime packages):

```bash
sudo dnf install gtk3 libayatana-appindicator3
```

On **Ubuntu/Debian**, see `packaging/appimage/install-deps-ubuntu.sh` for the package list used in CI.

Tests:

```bash
dotnet test
```

## AppImage

```bash
chmod +x TransmissionNET-*-x86_64.AppImage
./TransmissionNET-*-x86_64.AppImage
```

## License

GPL-3.0-or-later — see [LICENSE](LICENSE).
