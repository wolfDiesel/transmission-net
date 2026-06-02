export const OVERLAY_ROOT_ID = 'overlay-root'

const OVERLAY_ROOT_STYLE_ID = 'overlay-root-style'

export const OVERLAY_Z_INDEX = {
  elevated: 10100,
} as const

export function ensureOverlayRoot(): HTMLElement {
  if (!document.getElementById(OVERLAY_ROOT_STYLE_ID)) {
    const style = document.createElement('style')
    style.id = OVERLAY_ROOT_STYLE_ID
    style.textContent = `
      #${OVERLAY_ROOT_ID} {
        position: fixed;
        inset: 0;
        z-index: 10000;
        pointer-events: none;
        isolation: isolate;
      }
      #${OVERLAY_ROOT_ID} > * {
        pointer-events: auto;
      }
    `
    document.head.appendChild(style)
  }

  let root = document.getElementById(OVERLAY_ROOT_ID)
  if (!root) {
    root = document.createElement('div')
    root.id = OVERLAY_ROOT_ID
    document.body.appendChild(root)
  }

  return root
}

export function getOverlayRoot(): HTMLElement {
  return document.getElementById(OVERLAY_ROOT_ID) ?? ensureOverlayRoot()
}
