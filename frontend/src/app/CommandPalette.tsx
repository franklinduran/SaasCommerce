import { ArrowDown, ArrowUp, CornerDownLeft, Search, X } from 'lucide-react'
import type { LucideIcon } from 'lucide-react'
import { useEffect, useMemo, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import { useNavigate } from 'react-router-dom'
import type { PermissionCode } from '@/shared/types/permissions'
import { cn } from '@/shared/utils/cn'

export type CommandPaletteItem = {
  groupLabel?: string
  icon: LucideIcon
  label: string
  path: string
  requiredPermission?: PermissionCode | PermissionCode[]
}

type CommandPaletteProps = {
  items: readonly CommandPaletteItem[]
  onClose: () => void
  userPermissions: readonly string[]
}

/**
 * Spotlight-style command palette. Trigger from the sidebar search button or
 * via Ctrl/Cmd+K. Filters navigation items by label / group, supports keyboard
 * navigation (↑/↓ + Enter), and closes on Esc or backdrop click.
 */
export function CommandPalette({ items, onClose, userPermissions }: Readonly<CommandPaletteProps>) {
  const navigate = useNavigate()
  const [query, setQuery] = useState('')
  const [activeIdx, setActiveIdx] = useState(0)
  const listRef = useRef<HTMLDivElement>(null)

  // Permission-aware filtering, then text search across label and group.
  const filtered = useMemo(() => {
    const allowed = items.filter((item) => hasPermission(item.requiredPermission, userPermissions))
    const q = query.trim().toLowerCase()
    if (!q) return allowed
    return allowed.filter((item) => {
      const haystack = `${item.label} ${item.groupLabel ?? ''}`.toLowerCase()
      return haystack.includes(q)
    })
  }, [items, query, userPermissions])

  // Reset highlighted index whenever the filtered list changes.
  useEffect(() => {
    setActiveIdx(0)
  }, [filtered])

  // Scroll the active row into view as the user arrow-navigates.
  useEffect(() => {
    const list = listRef.current
    if (!list) return
    const active = list.querySelector<HTMLElement>(`[data-cmd-idx="${activeIdx}"]`)
    active?.scrollIntoView({ block: 'nearest' })
  }, [activeIdx])

  // Keyboard navigation: ↑/↓ move highlight, Enter selects, Esc closes.
  useEffect(() => {
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'ArrowDown') {
        event.preventDefault()
        setActiveIdx((i) => Math.min(i + 1, filtered.length - 1))
      } else if (event.key === 'ArrowUp') {
        event.preventDefault()
        setActiveIdx((i) => Math.max(i - 1, 0))
      } else if (event.key === 'Enter') {
        event.preventDefault()
        const item = filtered[activeIdx]
        if (item) {
          navigate(item.path)
          onClose()
        }
      } else if (event.key === 'Escape') {
        event.preventDefault()
        onClose()
      }
    }
    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [filtered, activeIdx, navigate, onClose])

  function handleSelect(item: CommandPaletteItem) {
    navigate(item.path)
    onClose()
  }

  return createPortal(
    <div
      className="fixed inset-0 z-50 flex items-start justify-center bg-black/40 px-4 pt-[12vh] backdrop-blur-sm animate-in fade-in duration-150"
      onClick={onClose}
      role="presentation"
    >
      <div
        aria-label="Búsqueda"
        aria-modal="true"
        className="w-full max-w-lg overflow-hidden rounded-2xl bg-white shadow-2xl ring-1 ring-black/5 animate-in fade-in zoom-in-95 duration-200"
        onClick={(event) => event.stopPropagation()}
        role="dialog"
      >
        {/* Search input */}
        <div className="flex items-center gap-2.5 border-b border-gray-100 px-4">
          <Search aria-hidden="true" className="shrink-0 text-gray-400" size={16} />
          <input
            autoFocus
            className="h-12 flex-1 bg-transparent text-sm font-medium text-gray-900 outline-none placeholder:font-normal placeholder:text-gray-400"
            onChange={(event) => setQuery(event.target.value)}
            placeholder="Buscar páginas, ajustes, ayuda..."
            type="text"
            value={query}
          />
          {query && (
            <button
              aria-label="Limpiar búsqueda"
              className="flex h-6 w-6 cursor-pointer items-center justify-center rounded-md text-gray-400 transition-colors hover:bg-gray-100 hover:text-gray-700"
              onClick={() => setQuery('')}
              type="button"
            >
              <X aria-hidden="true" size={14} />
            </button>
          )}
        </div>

        {/* Results list */}
        <div className="max-h-80 overflow-y-auto py-1" ref={listRef}>
          {filtered.length === 0 ? (
            <p className="px-4 py-8 text-center text-sm text-gray-400">
              Sin resultados para "{query}"
            </p>
          ) : (
            filtered.map((item, i) => (
              <button
                className={cn(
                  'flex w-full cursor-pointer items-center gap-3 px-4 py-2 text-left text-sm transition-colors',
                  i === activeIdx
                    ? 'bg-gray-100 text-gray-900'
                    : 'text-gray-700 hover:bg-gray-50 hover:text-gray-900',
                )}
                data-cmd-idx={i}
                key={item.path}
                onClick={() => handleSelect(item)}
                onMouseEnter={() => setActiveIdx(i)}
                type="button"
              >
                <item.icon
                  aria-hidden="true"
                  className={cn('shrink-0', i === activeIdx ? 'text-gray-700' : 'text-gray-400')}
                  size={15}
                  strokeWidth={1.85}
                />
                <span className="flex-1 truncate font-medium">{item.label}</span>
                {item.groupLabel && (
                  <span className="text-[10px] font-semibold uppercase tracking-[0.1em] text-gray-400">
                    {item.groupLabel}
                  </span>
                )}
              </button>
            ))
          )}
        </div>

        {/* Keyboard hints footer */}
        <div className="flex items-center gap-4 border-t border-gray-100 bg-gray-50/50 px-4 py-2 text-[11px] text-gray-500">
          <span className="flex items-center gap-1">
            <Kbd>
              <ArrowUp size={9} strokeWidth={2.5} />
            </Kbd>
            <Kbd>
              <ArrowDown size={9} strokeWidth={2.5} />
            </Kbd>
            navegar
          </span>
          <span className="flex items-center gap-1">
            <Kbd>
              <CornerDownLeft size={9} strokeWidth={2.5} />
            </Kbd>
            abrir
          </span>
          <span className="ml-auto flex items-center gap-1">
            <Kbd>esc</Kbd>
            cerrar
          </span>
        </div>
      </div>
    </div>,
    document.body,
  )
}

function Kbd({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <kbd className="inline-flex h-4 min-w-4 items-center justify-center rounded bg-white px-1 font-mono text-[10px] font-semibold text-gray-500 ring-1 ring-gray-200">
      {children}
    </kbd>
  )
}

function hasPermission(
  required: PermissionCode | PermissionCode[] | undefined,
  userPermissions: readonly string[],
): boolean {
  if (!required) return true
  const list = Array.isArray(required) ? required : [required]
  return list.some((p) => userPermissions.includes(p))
}
