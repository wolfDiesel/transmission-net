#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
PKG="$ROOT/packaging/appimage"
WWW="$ROOT/src/TransmissonNET.App/wwwroot"
PUB="$ROOT/web/transmission-ui/public"

mkdir -p "$WWW" "$PUB"
for f in transmission-net.svg transmission-net-mark.svg transmission-net-tray.svg; do
  cp "$PKG/$f" "$WWW/$f"
  cp "$PKG/$f" "$PUB/$f"
done

render_png() {
  local svg="$1" out="$2" size="$3"
  if command -v rsvg-convert >/dev/null; then
    rsvg-convert -w "$size" -h "$size" "$svg" -o "$out"
  elif command -v magick >/dev/null; then
    magick -background none "$svg" -resize "${size}x${size}" "$out"
  elif command -v convert >/dev/null; then
    convert -background none "$svg" -resize "${size}x${size}" "$out"
  else
    echo "No SVG rasterizer (rsvg-convert/magick)" >&2
    return 1
  fi
}

render_png "$PKG/transmission-net.svg" "$WWW/transmission-net.png" 256
render_png "$PKG/transmission-net-tray.svg" "$WWW/transmission-net-tray.png" 128
cp "$WWW/transmission-net.png" "$PUB/transmission-net.png"
cp "$WWW/transmission-net-tray.png" "$PUB/transmission-net-tray.png"

echo "Brand icons synced to wwwroot and public."
