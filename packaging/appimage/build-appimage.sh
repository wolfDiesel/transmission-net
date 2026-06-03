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
PHOTINO_SYSTEM_GUI_EXCLUDES=(
  --exclude-library=libwebkit2gtk-4.1.so.0
  --exclude-library=libjavascriptcoregtk-4.1.so.0
  --exclude-library=libgtk-3.so.0
  --exclude-library=libgdk-3.so.0
  --exclude-library=libgdk_pixbuf-2.0.so.0
  --exclude-library=libglib-2.0.so.0
  --exclude-library=libgobject-2.0.so.0
  --exclude-library=libgio-2.0.so.0
  --exclude-library=libcairo.so.2
  --exclude-library=libpango-1.0.so.0
  --exclude-library=libatk-1.0.so.0
  --exclude-library=libharfbuzz.so.0
  --exclude-library=libmount.so.1
  --exclude-library=libblkid.so.1
  --exclude-library=libuuid.so.1
  --exclude-library=libselinux.so.1
  --exclude-library=libpcre2-8.so.0
  --exclude-library=libffi.so.8
)

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

strip_bundled_gui_libs() {
  echo "Using system GTK/WebKit — removing bundled GUI libs from AppDir ..."
  find "$APPDIR" -name 'libwebkit*.so*' -delete
  find "$APPDIR" -name 'libjavascriptcoregtk*.so*' -delete
  find "$APPDIR" -name 'libgtk-3.so*' -delete
  find "$APPDIR" -name 'libgdk-3.so*' -delete
  find "$APPDIR" -name 'libgdk_pixbuf-2.0.so*' -delete
  find "$APPDIR" -name 'libglib-2.0.so*' -delete
  find "$APPDIR" -name 'libgobject-2.0.so*' -delete
  find "$APPDIR" -name 'libgio-2.0.so*' -delete
  find "$APPDIR" -name 'libcairo.so*' -delete
  find "$APPDIR" -name 'libpango*.so*' -delete
  find "$APPDIR" -name 'libatk*.so*' -delete
  find "$APPDIR" -name 'libharfbuzz.so*' -delete
  find "$APPDIR" -name 'libmount.so*' -delete
  find "$APPDIR" -name 'libblkid.so*' -delete
  find "$APPDIR" -name 'libuuid.so*' -delete
  find "$APPDIR" -name 'libselinux.so*' -delete
  find "$APPDIR" -name 'libpcre2-8.so*' -delete
  find "$APPDIR" -name 'libffi.so*' -delete
  find "$APPDIR" -type d -name 'webkit2gtk-4.1' -prune -exec rm -rf {} +
}

strip_conflicting_usr_lib() {
  if [ -d "$APPDIR/usr/lib" ]; then
    find "$APPDIR/usr/lib" -mindepth 1 -maxdepth 1 -exec rm -rf {} +
  fi
}

verify_system_gui_only() {
  if find "$APPDIR" -name 'libwebkit2gtk-4.1.so.0' | grep -q .; then
    echo "Bundled libwebkit2gtk must not be in the AppImage" >&2
    exit 1
  fi
  if find "$APPDIR" -name 'libgtk-3.so.0' | grep -q .; then
    echo "Bundled libgtk-3 must not be in the AppImage (use system GTK on Fedora)" >&2
    exit 1
  fi
}

verify_publish_wwwroot() {
  if [ ! -f "$PUBLISH/wwwroot/index.html" ]; then
    echo "wwwroot/index.html missing from dotnet publish output" >&2
    exit 1
  fi
}

install_custom_apprun() {
  cp "$PKG/AppRun" "$APPDIR/AppRun"
  chmod +x "$APPDIR/AppRun"
  cp "$PKG/transmission-net.svg" "$APPDIR/transmission-net.svg"
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
    echo "CI builder is missing libraries required by Photino.Native.so:" >&2
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

run_linuxdeploy() {
  local icon="$1"
  local deploy="$LINUXDEPLOY"
  local main_bin="$APP_BIN_DIR/TransmissonNET.App"
  local photino_so="$APP_BIN_DIR/Photino.Native.so"

  echo "Bundling .NET deps only (system GTK/WebKit at runtime) ..."
  "$deploy" --appdir="$APPDIR" \
    --executable="$main_bin" \
    --deploy-deps-only="$photino_so" \
    "${PHOTINO_SYSTEM_GUI_EXCLUDES[@]}" \
    --desktop-file="$PKG/transmission-net.desktop" \
    --icon-file="$icon"

  strip_bundled_gui_libs
  strip_conflicting_usr_lib
  verify_system_gui_only
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
  verify_publish_wwwroot
  verify_photino_runtime
  assemble_appdir
  run_linuxdeploy "$icon"
  finalize
}

main "$@"
