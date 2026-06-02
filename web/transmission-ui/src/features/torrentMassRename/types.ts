export type MassRenameMode =
  | 'findReplace'
  | 'prefixSuffix'
  | 'numbering'
  | 'regex'
  | 'template'

export type MassRenameSort = 'path' | 'name'

export type ScopeFile = {
  path: string
  basename: string
  stem: string
  ext: string
  relativePath: string
}

export type RenamePlanEntry = {
  path: string
  oldName: string
  newName: string
  changed: boolean
}

export type MassRenameRule = {
  mode: MassRenameMode
  find: string
  replace: string
  caseSensitive: boolean
  prefix: string
  suffix: string
  numberingTemplate: string
  numberingStart: number
  numberingStep: number
  regexPattern: string
  regexReplacement: string
  regexFlags: string
  template: string
  stemOnly: boolean
  sort: MassRenameSort
}

export type PlanValidation = {
  canApply: boolean
  errors: string[]
  warnings: string[]
}
