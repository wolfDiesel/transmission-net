export type LocaleMessages = Record<string, unknown>

export type LocaleCode = 'en' | 'ru'

export function translate(
  messages: LocaleMessages,
  key: string,
  params?: Record<string, string | number>,
): string {
  const value = resolvePath(messages, key)
  if (typeof value !== 'string') return key
  if (!params) return value
  return value.replace(/\{(\w+)\}/g, (_, name: string) => {
    const replacement = params[name]
    return replacement === undefined ? `{${name}}` : String(replacement)
  })
}

function resolvePath(messages: LocaleMessages, key: string): unknown {
  let current: unknown = messages
  for (const part of key.split('.')) {
    if (current === null || typeof current !== 'object') return undefined
    current = (current as Record<string, unknown>)[part]
  }
  return current
}

export function translateList(messages: LocaleMessages, key: string): string[] {
  const value = resolvePath(messages, key)
  return Array.isArray(value) && value.every((item) => typeof item === 'string')
    ? (value as string[])
    : []
}
