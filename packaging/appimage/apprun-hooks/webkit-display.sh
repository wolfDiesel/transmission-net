#!/bin/sh
export LD_LIBRARY_PATH="${APPDIR}/usr/lib:${APPDIR}/usr/lib/x86_64-linux-gnu:${LD_LIBRARY_PATH:-}"
export GTK_EXE_PREFIX="${APPDIR}"
export GTK_DATA_PREFIX="${APPDIR}"
export XDG_DATA_DIRS="${APPDIR}/usr/share:${XDG_DATA_DIRS:-/usr/local/share:/usr/share}"
webkit_dir="${APPDIR}/usr/lib/webkit2gtk-4.1"
if [ -d "${webkit_dir}/injected-bundle" ]; then
  export WEBKIT_INJECTED_BUNDLE_PATH="${webkit_dir}/injected-bundle"
fi
export GDK_BACKEND="${GDK_BACKEND:-x11}"
export WEBKIT_DISABLE_DMABUF_RENDERER="${WEBKIT_DISABLE_DMABUF_RENDERER:-1}"
export WEBKIT_DISABLE_SANDBOX="${WEBKIT_DISABLE_SANDBOX:-1}"
export WEBKIT_DISABLE_COMPOSITING_MODE="${WEBKIT_DISABLE_COMPOSITING_MODE:-1}"
if [ "$GSK_RENDERER" = "vulkan" ]; then
  export GSK_RENDERER=ngl
fi
