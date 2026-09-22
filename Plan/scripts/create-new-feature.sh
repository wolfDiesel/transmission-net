#!/usr/bin/env bash
set -euo pipefail

# Создаёт каркас новой спеки Plan/specs/NNN-slug/ из шаблонов Plan/templates/.
# Использование:
#   Plan/scripts/create-new-feature.sh my-feature

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
templates="$root/templates"
specs="$root/specs"

slug="${1:-}"
if [[ -z "$slug" ]]; then
  echo "usage: $(basename "$0") <slug>      (например: $(basename "$0") my-feature)" >&2
  exit 1
fi

# Следующий порядковый номер NNN.
last="$(ls -1 "$specs" 2>/dev/null | grep -E '^[0-9]{3}-' | sort | tail -n1 | cut -c1-3)"
if [[ -z "$last" || ! "$last" =~ ^[0-9]{3}$ ]]; then
  nnn="000"
else
  nnn="$(printf '%03d' $((10#$last + 1)))"
fi

dir="$specs/$nnn-$slug"
if [[ -d "$dir" ]]; then
  echo "Уже существует: $dir" >&2
  exit 1
fi

mkdir -p "$dir"
for f in spec plan tasks; do
  cp "$templates/$f-template.md" "$dir/$f.md"
done

echo "Создано: $dir (spec.md, plan.md, tasks.md)"