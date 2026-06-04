import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  type ReactNode,
} from 'react'
import { getLocaleMessages } from './locales'
import { translate, translateList, type LocaleCode } from './translate'

export type TranslateFn = (key: string, params?: Record<string, string | number>) => string

type I18nContextValue = {
  locale: LocaleCode
  t: TranslateFn
  tList: (key: string) => string[]
}

const I18nContext = createContext<I18nContextValue | null>(null)

export function I18nProvider({
  locale,
  children,
}: {
  locale: LocaleCode
  children: ReactNode
}) {
  const messages = useMemo(() => getLocaleMessages(locale), [locale])

  const t = useCallback<TranslateFn>(
    (key, params) => translate(messages, key, params),
    [messages],
  )

  const tList = useCallback((key: string) => translateList(messages, key), [messages])

  useEffect(() => {
    document.documentElement.lang = locale
  }, [locale])

  const value = useMemo(() => ({ locale, t, tList }), [locale, t, tList])

  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>
}

export function useI18n() {
  const ctx = useContext(I18nContext)
  if (!ctx) throw new Error('useI18n must be used within I18nProvider')
  return ctx
}

export function normalizeLocale(value: string | undefined): LocaleCode {
  return value === 'ru' ? 'ru' : 'en'
}
