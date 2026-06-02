import { compileRegex } from './regexUtils'
import type { MassRenameRule, ScopeFile } from './types'

function applyFindReplace(
  text: string,
  find: string,
  replace: string,
  caseSensitive: boolean,
): string {
  if (!find) return text
  if (caseSensitive) return text.split(find).join(replace)
  const escaped = find.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
  return text.replace(new RegExp(escaped, 'gi'), replace)
}

function applyRegex(text: string, pattern: string, replacement: string, flags: string): string {
  const compiled = compileRegex(pattern, flags)
  if (!compiled.ok) return text
  return text.replace(compiled.regex, replacement)
}

export function regexRenameTarget(file: ScopeFile): string {
  return file.basename
}

function applyNumberingTemplate(template: string, index: number, start: number, step: number): string {
  const n = start + index * step
  return template.replace(/\{n(?::(\d+))?\}/g, (_, pad) =>
    pad ? String(n).padStart(Number(pad), '0') : String(n),
  )
}

function applyTemplate(
  template: string,
  file: ScopeFile,
  index: number,
  rule: MassRenameRule,
): string {
  const n = rule.numberingStart + index * rule.numberingStep
  return template
    .replace(/\{name\}/g, file.stem)
    .replace(/\{ext\}/g, file.ext)
    .replace(/\{basename\}/g, file.basename)
    .replace(/\{path\}/g, file.relativePath)
    .replace(/\{n(?::(\d+))?\}/g, (_, pad) => (pad ? String(n).padStart(Number(pad), '0') : String(n)))
}

export function computeNewBasename(file: ScopeFile, index: number, rule: MassRenameRule): string {
  const target = rule.stemOnly ? file.stem : file.basename

  let result = target

  switch (rule.mode) {
    case 'findReplace':
      result = applyFindReplace(target, rule.find, rule.replace, rule.caseSensitive)
      break
    case 'prefixSuffix':
      result = `${rule.prefix}${target}${rule.suffix}`
      break
    case 'numbering':
      result = applyNumberingTemplate(
        rule.numberingTemplate || '{n}',
        index,
        rule.numberingStart,
        rule.numberingStep,
      )
      break
    case 'regex':
      result = applyRegex(
        regexRenameTarget(file),
        rule.regexPattern,
        rule.regexReplacement,
        rule.regexFlags,
      )
      break
    case 'template':
      result = applyTemplate(rule.template || '{name}', file, index, rule)
      break
  }

  if (rule.stemOnly && rule.mode !== 'regex') return `${result}${file.ext}`
  return result
}
