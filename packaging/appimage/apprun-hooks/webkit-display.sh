#!/bin/sh
if [ -n "${APPDIR:-}" ]; then
  export LD_LIBRARY_PATH="${APPDIR}/usr/lib:${APPDIR}/usr/lib/x86_64-linux-gnu:${LD_LIBRARY_PATH:-}"
  webkit_dir="${APPDIR}/usr/lib/x86_64-linux-gnu/webkit2gtk-4.1"
  if [ -d "${webkit_dir}/injected-bundle" ]; then
    export WEBKIT_INJECTED_BUNDLE_PATH="${webkit_dir}/injected-bundle"
  fi
fi
export WEBKIT_DISABLE_DMABUF_RENDERER="${WEBKIT_DISABLE_DMABUF_RENDERER:-1}"
if [ "$GSK_RENDERER" = "vulkan" ]; then
  export GSK_RENDERER=ngl
fi
