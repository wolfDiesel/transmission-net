#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
export WEBKIT_DISABLE_DMABUF_RENDERER="${WEBKIT_DISABLE_DMABUF_RENDERER:-1}"
export GSK_RENDERER="${GSK_RENDERER:-ngl}"
exec dotnet run --project "$ROOT/src/TransmissonNET.App" --launch-profile TransmissonNET "$@"
