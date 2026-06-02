#!/bin/sh
export WEBKIT_DISABLE_DMABUF_RENDERER="${WEBKIT_DISABLE_DMABUF_RENDERER:-1}"
if [ "$GSK_RENDERER" = "vulkan" ]; then
  export GSK_RENDERER=ngl
fi
