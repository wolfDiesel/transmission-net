import type { PlanValidation, RenamePlanEntry } from './types'

const ILLEGAL_NAME_CHARS = /[\\/:*?"<>|]/

function parentPath(path: string): string {
  const idx = path.lastIndexOf('/')
  return idx < 0 ? '' : path.slice(0, idx)
}

export function validatePlan(plan: RenamePlanEntry[]): PlanValidation {
  const errors: string[] = []
  const warnings: string[] = []
  const changed = plan.filter((e) => e.changed)

  if (changed.length === 0) {
    warnings.push('No files would be renamed with the current rules.')
  }

  const targetByFolder = new Map<string, Map<string, string[]>>()

  for (const entry of changed) {
    if (!entry.newName.trim()) {
      errors.push(`Empty name for ${entry.path}`)
      continue
    }

    if (ILLEGAL_NAME_CHARS.test(entry.newName)) {
      errors.push(`Invalid characters in "${entry.newName}" (${entry.path})`)
    }

    const folder = parentPath(entry.path)
    const folderMap = targetByFolder.get(folder) ?? new Map<string, string[]>()
    const list = folderMap.get(entry.newName) ?? []
    list.push(entry.path)
    folderMap.set(entry.newName, list)
    targetByFolder.set(folder, folderMap)
  }

  for (const [folder, names] of targetByFolder) {
    for (const [name, paths] of names) {
      if (paths.length > 1) {
        const label = folder ? `${folder}/` : ''
        errors.push(`Duplicate name "${label}${name}" for ${paths.length} files`)
      }
    }
  }

  if (changed.length > 200) {
    warnings.push(`${changed.length} files will be renamed. This may take a while.`)
  }

  return {
    canApply: errors.length === 0 && changed.length > 0,
    errors,
    warnings,
  }
}
