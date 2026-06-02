#!/bin/sh
export LD_LIBRARY_PATH="${APPDIR}/usr/bin:${APPDIR}/usr/lib:${LD_LIBRARY_PATH:-}"
export LANG="${LANG:-en_US.UTF-8}"
export LC_ALL="${LC_ALL:-$LANG}"
if [ -f /etc/fonts/fonts.conf ]; then
  export FONTCONFIG_FILE=/etc/fonts/fonts.conf
  export FONTCONFIG_PATH=/etc/fonts
fi
export GDK_BACKEND="${GDK_BACKEND:-x11}"
export WEBKIT_DISABLE_DMABUF_RENDERER="${WEBKIT_DISABLE_DMABUF_RENDERER:-1}"
export WEBKIT_DISABLE_SANDBOX="${WEBKIT_DISABLE_SANDBOX:-1}"
export WEBKIT_DISABLE_COMPOSITING_MODE="${WEBKIT_DISABLE_COMPOSITING_MODE:-1}"
export LIBGL_ALWAYS_SOFTWARE="${LIBGL_ALWAYS_SOFTWARE:-1}"
if [ "$GSK_RENDERER" = "vulkan" ]; then
  export GSK_RENDERER=ngl
fi
