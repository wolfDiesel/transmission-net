import type { LocaleCode } from '../translate'
import { en } from './en'
import { ru } from './ru'

export const locales = { en, ru } as const satisfies Record<LocaleCode, typeof en>

export function getLocaleMessages(code: LocaleCode) {
  return locales[code] ?? en
}
