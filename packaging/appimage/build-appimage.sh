#!/usr/bin/env bash
# CI packaging entrypoint (GitHub Actions release workflow). Not intended for local use.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
PKG="$ROOT/packaging/appimage"
BUILD="$ROOT/build/appimage"
APPDIR="$BUILD/AppDir"
PUBLISH="$BUILD/publish"
TOOLS="$BUILD/tools"

export ARCH="${ARCH:-x86_64}"
APPIMAGE_VERSION="${APPIMAGE_VERSION:-0.0.0}"
OUTPUT_NAME="${OUTPUT_NAME:-TransmissionNET-${APPIMAGE_VERSION}-x86_64.AppImage}"
export APPIMAGE_EXTRACT_AND_RUN=1

need_cmd() {
  command -v "$1" >/dev/null 2>&1 || {
    echo "Missing required command: $1" >&2
    exit 1
  }
}

ensure_tools() {
  mkdir -p "$TOOLS"
  local deploy="$TOOLS/linuxdeploy-${ARCH}.AppImage"
  local gtk_plugin="$TOOLS/linuxdeploy-plugin-gtk.sh"
  if [ ! -f "$deploy" ]; then
    echo "Downloading linuxdeploy..."
    curl -fsSL -o "$deploy" \
      "https://github.com/linuxdeploy/linuxdeploy/releases/download/continuous/linuxdeploy-${ARCH}.AppImage"
    chmod +x "$deploy"
  fi
  if [ ! -f "$gtk_plugin" ]; then
    echo "Downloading linuxdeploy-plugin-gtk..."
    curl -fsSL -o "$gtk_plugin" \
      "https://raw.githubusercontent.com/linuxdeploy/linuxdeploy-plugin-gtk/master/linuxdeploy-plugin-gtk.sh"
    chmod +x "$gtk_plugin"
  fi
  export PATH="$TOOLS:$PATH"
  export LINUXDEPLOY="$deploy"
}

prepare_icon() {
  local icon="$BUILD/transmission-net.png"
  mkdir -p "$BUILD"
  if [ -f "$icon" ] && [ ! "$PKG/transmission-net.svg" -nt "$icon" ]; then
    echo "$icon"
    return
  fi
  if command -v rsvg-convert >/dev/null 2>&1; then
    rsvg-convert -w 512 -h 512 "$PKG/transmission-net.svg" -o "$icon"
  elif command -v magick >/dev/null 2>&1; then
    magick -background none "$PKG/transmission-net.svg" -resize 512x512 "$icon"
  elif command -v convert >/dev/null 2>&1; then
    convert -background none "$PKG/transmission-net.svg" -resize 512x512 "$icon"
  else
    echo "Install librsvg2-tools or ImageMagick to render the icon (rsvg-convert / magick)." >&2
    exit 1
  fi
  echo "$icon"
}

webkit_process_dir() {
  if [ -n "${WEBKIT_PROCESS_DIR:-}" ] && [ -d "$WEBKIT_PROCESS_DIR" ]; then
    echo "$WEBKIT_PROCESS_DIR"
    return
  fi
  local libdir
  libdir="$(pkg-config --variable=libdir webkit2gtk-4.1 2>/dev/null || true)"
  if [ -n "$libdir" ] && [ -d "$libdir/webkit2gtk-4.1" ]; then
    echo "$libdir/webkit2gtk-4.1"
    return
  fi
  for candidate in \
    /usr/lib64/webkit2gtk-4.1 \
    /usr/lib/x86_64-linux-gnu/webkit2gtk-4.1 \
    /usr/lib/webkit2gtk-4.1; do
    if [ -d "$candidate" ]; then
      echo "$candidate"
      return
    fi
  done
  echo "webkit2gtk-4.1 process directory not found. Install webkit2gtk4.1-devel / webkit2gtk4.1." >&2
  exit 1
}

find_appdir_webkit_dir() {
  find "$APPDIR" -type d -name 'webkit2gtk-4.1' 2>/dev/null | head -1
}

publish_app() {
  echo "Publishing .NET app (linux-x64, self-contained)..."
  rm -rf "$PUBLISH"
  dotnet publish "$ROOT/src/TransmissonNET.App/TransmissonNET.App.csproj" \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:PublishSingleFile=false \
    -p:DebugType=none \
    -p:DebugSymbols=false \
    -o "$PUBLISH"
}

verify_photino_runtime() {
  local photino="$PUBLISH/Photino.Native.so"
  [ -f "$photino" ] || {
    echo "Photino.Native.so not found after publish" >&2
    exit 1
  }
  local missing
  missing="$(ldd "$photino" | grep 'not found' || true)"
  if [ -n "$missing" ]; then
    echo "Host is missing libraries required by Photino.Native.so:" >&2
    echo "$missing" >&2
    echo "On Ubuntu run: packaging/appimage/install-deps-ubuntu.sh" >&2
    exit 1
  fi
}

assemble_appdir() {
  echo "Assembling AppDir..."
  rm -rf "$APPDIR"
  local app_home="$APPDIR/opt/TransmissonNET"
  mkdir -p "$app_home" "$APPDIR/apprun-hooks"
  cp -a "$PUBLISH/." "$app_home/"
  rm -f \
    "$app_home/createdump" \
    "$app_home/libcoreclrtraceptprovider.so" \
    "$app_home/libmscordbi.so" \
    "$app_home/libmscordaccore.so"
  find "$app_home" -name '*.so' -exec chmod -x {} +
  chmod +x "$app_home/TransmissonNET.App" "$app_home/Photino.Native.so"
  cp "$PKG/transmission-net.desktop" "$APPDIR/transmission-net.desktop"
  cp "$PKG/apprun-hooks/webkit-display.sh" "$APPDIR/apprun-hooks/"
  chmod +x "$APPDIR/apprun-hooks/webkit-display.sh"
}

app_executable() {
  echo "$APPDIR/opt/TransmissonNET/TransmissonNET.App"
}

photino_library() {
  echo "$APPDIR/opt/TransmissonNET/Photino.Native.so"
}

webkit_deploy_args() {
  local dir="$1"
  local args=()
  for name in WebKitNetworkProcess WebKitWebProcess WebKitGPUProcess; do
    if [ -x "$dir/$name" ]; then
      args+=(--deploy-deps-only="$dir/$name")
    fi
  done
  printf '%s\n' "${args[@]}"
}

run_linuxdeploy() {
  local icon="$1"
  local deploy="$LINUXDEPLOY"
  local gtk_plugin="$TOOLS/linuxdeploy-plugin-gtk.sh"
  local webkit_sys
  webkit_sys="$(webkit_process_dir)"

  local main_bin photino_so
  main_bin="$(app_executable)"
  photino_so="$(photino_library)"

  echo "Bundling GTK/WebKit dependencies..."
  "$deploy" --appdir="$APPDIR" \
    --executable="$main_bin" \
    --deploy-deps-only="$photino_so" \
    --desktop-file="$APPDIR/transmission-net.desktop" \
    --icon-file="$icon" \
    --plugin gtk \
    --output appimage

  local webkit_app
  webkit_app="$(find_appdir_webkit_dir)"
  if [ -z "$webkit_app" ]; then
    mkdir -p "$APPDIR/usr/lib"
    local dest="$APPDIR/usr/lib/webkit2gtk-4.1"
    cp -a "$webkit_sys/." "$dest/"
    webkit_app="$dest"
  fi

  mapfile -t extra < <(webkit_deploy_args "$webkit_app")
  if [ "${#extra[@]}" -gt 0 ]; then
    echo "Deploying WebKit helper process dependencies..."
    "$deploy" --appdir="$APPDIR" "${extra[@]}" --output appimage
  fi
}

finalize() {
  local produced
  produced="$(find "$BUILD" -maxdepth 1 -name '*.AppImage' -type f -printf '%T@ %p\n' | sort -rn | head -1 | cut -d' ' -f2-)"
  if [ -z "$produced" ]; then
    echo "AppImage was not produced under $BUILD" >&2
    exit 1
  fi
  local out="$ROOT/dist/$OUTPUT_NAME"
  mkdir -p "$ROOT/dist"
  mv -f "$produced" "$out"
  chmod +x "$out"
  echo "AppImage: $out"
  ls -lh "$out"
}

main() {
  need_cmd dotnet
  need_cmd curl
  need_cmd pkg-config
  pkg-config --exists webkit2gtk-4.1 || {
    echo "Install webkit2gtk4.1 (and -devel for pkg-config)." >&2
    exit 1
  }

  ensure_tools
  local icon
  icon="$(prepare_icon)"
  publish_app
  verify_photino_runtime
  assemble_appdir
  run_linuxdeploy "$icon"
  finalize
}

main "$@"
