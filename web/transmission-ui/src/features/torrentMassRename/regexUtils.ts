export function normalizeRegexFlags(flags: string): string {
  return flags.replace(/[^gimsuy]/g, '')
}

export function compileRegex(
  pattern: string,
  flags: string,
): { ok: true; regex: RegExp } | { ok: false; error: string } {
  if (!pattern.trim()) {
    return { ok: false, error: 'Enter a regex pattern' }
  }

  try {
    return { ok: true, regex: new RegExp(pattern, normalizeRegexFlags(flags)) }
  } catch (error) {
    const message = error instanceof Error ? error.message : 'Invalid pattern'
    return { ok: false, error: message }
  }
}

export function regexMatchesText(regex: RegExp, text: string): boolean {
  const probe = new RegExp(regex.source, normalizeRegexFlags(regex.flags))
  return probe.test(text)
}
