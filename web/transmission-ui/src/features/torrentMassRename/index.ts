export { buildRenamePlan } from './buildRenamePlan'
export { collectScopeFiles, formatScopeLabel, TORRENT_ROOT_SCOPE } from './collectScopeFiles'
export { regexRenameTarget } from './applyRule'
export { compileRegex } from './regexUtils'
export { validateMassRenameRule } from './validateMassRenameRule'
export { validatePlan } from './validatePlan'
export type {
  MassRenameMode,
  MassRenameRule,
  MassRenameSort,
  PlanValidation,
  RenamePlanEntry,
  ScopeFile,
} from './types'

import type { MassRenameRule } from './types'

export const defaultMassRenameRule = (): MassRenameRule => ({
  mode: 'regex',
  find: '',
  replace: '',
  caseSensitive: false,
  prefix: '',
  suffix: '',
  numberingTemplate: '{n:02} - {name}',
  numberingStart: 1,
  numberingStep: 1,
  regexPattern: '',
  regexReplacement: '',
  regexFlags: 'g',
  template: '{n:02} - {name}',
  stemOnly: true,
  sort: 'path',
})
