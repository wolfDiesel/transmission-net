import { compileRegex, regexMatchesText } from './regexUtils'
import { regexRenameTarget } from './applyRule'
import type { MassRenameRule, ScopeFile } from './types'

export function validateMassRenameRule(rule: MassRenameRule, files: ScopeFile[]): string[] {
  if (rule.mode !== 'regex') return []

  const compiled = compileRegex(rule.regexPattern, rule.regexFlags)
  if (!compiled.ok) return [compiled.error]

  const hasMatch = files.some((file) => regexMatchesText(compiled.regex, regexRenameTarget(file)))
  if (!hasMatch) {
    return [
      'Pattern matches no files in scope. Regex is applied to the full file name (with extension), e.g. Genocyber 01 ….mkv',
    ]
  }

  return []
}
