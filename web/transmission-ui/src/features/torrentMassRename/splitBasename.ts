export function splitBasename(basename: string): { stem: string; ext: string } {
  if (!basename) return { stem: '', ext: '' }
  const dot = basename.lastIndexOf('.')
  if (dot <= 0) return { stem: basename, ext: '' }
  return { stem: basename.slice(0, dot), ext: basename.slice(dot) }
}
