import type { MassRenameRule, RenamePlanEntry, ScopeFile } from './types'
import { computeNewBasename } from './applyRule'

function sortFiles(files: ScopeFile[], sort: MassRenameRule['sort']): ScopeFile[] {
  const copy = [...files]
  if (sort === 'name') {
    copy.sort((a, b) => a.basename.localeCompare(b.basename))
    return copy
  }
  copy.sort((a, b) => a.path.localeCompare(b.path))
  return copy
}

export function buildRenamePlan(files: ScopeFile[], rule: MassRenameRule): RenamePlanEntry[] {
  const sorted = sortFiles(files, rule.sort)
  return sorted.map((file, index) => {
    const newName = computeNewBasename(file, index, rule)
    return {
      path: file.path,
      oldName: file.basename,
      newName,
      changed: newName !== file.basename,
    }
  })
}
