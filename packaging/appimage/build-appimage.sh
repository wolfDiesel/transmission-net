#!/usr/bin/env bash
# CI packaging entrypoint (GitHub Actions release workflow). Not intended for local use.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
PKG="$ROOT/packaging/appimage"
BUILD="$ROOT/build/appimage"
APPDIR="$BUILD/AppDir"
PUBLISH="$BUILD/publish"
TOOLS="$BUILD/tools"
APP_BIN_DIR="$APPDIR/usr/bin"

export ARCH="${ARCH:-x86_64}"
APPIMAGE_VERSION="${APPIMAGE_VERSION:-0.0.0}"
OUTPUT_NAME="${OUTPUT_NAME:-TransmissionNET-${APPIMAGE_VERSION}-x86_64.AppImage}"
export APPIMAGE_EXTRACT_AND_RUN=1
export DEPLOY_GTK_VERSION=3

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
  local appimagetool="$TOOLS/appimagetool-${ARCH}.AppImage"
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
  if [ ! -f "$appimagetool" ]; then
    echo "Downloading appimagetool..."
    curl -fsSL -o "$appimagetool" \
      "https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-${ARCH}.AppImage"
    chmod +x "$appimagetool"
  fi
  export PATH="$TOOLS:$PATH"
  export LINUXDEPLOY="$deploy"
  export APPIMAGETOOL="$appimagetool"
}

strip_appdir_acls() {
  if command -v setfacl >/dev/null 2>&1; then
    find "$APPDIR" -exec setfacl -b {} + 2>/dev/null || true
  fi
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

appdir_webkit_process_dir() {
  echo "$APPDIR/usr/lib/webkit2gtk-4.1"
}

bundle_webkit_processes() {
  local src="$1"
  local dest
  dest="$(appdir_webkit_process_dir)"
  echo "Bundling WebKit helper processes into $dest ..."
  rm -rf "$dest"
  mkdir -p "$dest"
  cp -a "$src/." "$dest/"
  chmod +x "$dest"/WebKitNetworkProcess "$dest"/WebKitWebProcess 2>/dev/null || true
  if [ -x "$dest/WebKitGPUProcess" ]; then
    chmod +x "$dest/WebKitGPUProcess"
  fi
}

patch_file_webkit_paths() {
  local file="$1"
  [ -f "$file" ] || return 0
  sed -i \
    -e 's|/usr/lib/x86_64-linux-gnu/webkit2gtk-4.1|webkit2gtk-4.1|g' \
    -e 's|/usr/lib64/webkit2gtk-4.1|webkit2gtk-4.1|g' \
    -e 's|/usr/lib/webkit2gtk-4.1|webkit2gtk-4.1|g' \
    "$file" 2>/dev/null || true
}

patch_webkit_libraries() {
  echo "Patching WebKit paths (relative to usr/lib) ..."
  find "$APPDIR/usr/lib/webkit2gtk-4.1" -type f ! -type l 2>/dev/null | while read -r f; do
    patch_file_webkit_paths "$f"
  done
  find "$APPDIR/usr/lib" -maxdepth 1 \( -name 'libwebkit*.so*' -o -name 'libjavascriptcoregtk*.so*' \) -type f ! -type l | while read -r lib; do
    patch_file_webkit_paths "$lib"
  done
}

verify_webkit_path_patch() {
  local lib
  lib="$(find "$APPDIR/usr/lib" -maxdepth 1 -name 'libwebkit2gtk-4.1.so.0' -type f | head -1)"
  if [ -z "$lib" ]; then
    echo "libwebkit2gtk-4.1.so.0 not found in AppDir" >&2
    exit 1
  fi
  if strings "$lib" | grep -q '/usr/lib/.*/webkit2gtk-4.1'; then
    echo "libwebkit still contains absolute /usr paths after patch:" >&2
    strings "$lib" | grep '/usr/lib/.*/webkit2gtk-4.1' | head -3 >&2
    exit 1
  fi
}

verify_bundled_webkit_processes() {
  local dest
  dest="$(appdir_webkit_process_dir)"
  for name in WebKitNetworkProcess WebKitWebProcess; do
    if [ ! -x "$dest/$name" ]; then
      echo "Missing bundled WebKit helper: $dest/$name" >&2
      exit 1
    fi
  done
}

install_custom_apprun() {
  cp "$PKG/AppRun" "$APPDIR/AppRun"
  chmod +x "$APPDIR/AppRun"
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
  mkdir -p "$APP_BIN_DIR" "$APPDIR/apprun-hooks"
  cp -a "$PUBLISH/." "$APP_BIN_DIR/"
  rm -f \
    "$APP_BIN_DIR/createdump" \
    "$APP_BIN_DIR/libcoreclrtraceptprovider.so" \
    "$APP_BIN_DIR/libmscordbi.so" \
    "$APP_BIN_DIR/libmscordaccore.so"
  find "$APP_BIN_DIR" -name '*.so' -exec chmod -x {} +
  chmod +x "$APP_BIN_DIR/TransmissonNET.App" "$APP_BIN_DIR/Photino.Native.so"
  cp "$PKG/apprun-hooks/webkit-display.sh" "$APPDIR/apprun-hooks/"
  chmod +x "$APPDIR/apprun-hooks/webkit-display.sh"
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
  local main_bin="$APP_BIN_DIR/TransmissonNET.App"
  local photino_so="$APP_BIN_DIR/Photino.Native.so"
  local webkit_sys
  webkit_sys="$(webkit_process_dir)"

  echo "Bundling GTK/WebKit dependencies (pass 1)..."
  "$deploy" --appdir="$APPDIR" \
    --executable="$main_bin" \
    --deploy-deps-only="$photino_so" \
    --desktop-file="$PKG/transmission-net.desktop" \
    --icon-file="$icon" \
    --plugin gtk

  bundle_webkit_processes "$webkit_sys"

  mapfile -t extra < <(webkit_deploy_args "$(appdir_webkit_process_dir)")
  if [ "${#extra[@]}" -gt 0 ]; then
    echo "Deploying WebKit helper process dependencies (pass 2)..."
    "$deploy" --appdir="$APPDIR" "${extra[@]}"
  fi

  patch_webkit_libraries
  verify_webkit_path_patch
  verify_bundled_webkit_processes
  install_custom_apprun

  local out="$ROOT/dist/$OUTPUT_NAME"
  mkdir -p "$ROOT/dist"
  rm -f "$out"

  echo "Creating AppImage at $out ..."
  strip_appdir_acls
  "$APPIMAGETOOL" "$APPDIR" "$out"
  chmod +x "$out"
}

finalize() {
  local out="$ROOT/dist/$OUTPUT_NAME"
  if [ ! -f "$out" ]; then
    echo "AppImage was not created: $out" >&2
    find "$ROOT" "$BUILD" -name '*.AppImage' -type f 2>/dev/null || true
    exit 1
  fi
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
