import { ChevronRight } from 'lucide-react'
import { useEffect, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import { NavLink } from 'react-router-dom'
import type { NavigationItem } from '@/app/navigation'
import { cn } from '@/shared/utils/cn'

// ─── NavigationSection ────────────────────────────────────────────────────────
// Renders one navigation group: either as an accordion (expanded sidebar) or
// as a single icon button that opens a portaled popup (collapsed sidebar).

type NavigationSectionProps = {
  collapsed: boolean
  expanded: boolean
  items: readonly NavigationItem[]
  label?: string
  onPopupCancelClose?: () => void
  onPopupClose?: () => void
  onPopupHoverEnter?: () => void
  onPopupHoverLeave?: () => void
  onPopupToggle?: () => void
  onToggle?: () => void
  popupOpen?: boolean
  userPermissions: string[]
}

export function NavigationSection({
  collapsed,
  expanded,
  items,
  label,
  onPopupCancelClose,
  onPopupClose,
  onPopupHoverEnter,
  onPopupHoverLeave,
  onPopupToggle,
  onToggle,
  popupOpen,
  userPermissions,
}: Readonly<NavigationSectionProps>) {
  const visibleItems = items.filter((item) => {
    if (!item.requiredPermission) return true
    const required = Array.isArray(item.requiredPermission)
      ? item.requiredPermission
      : [item.requiredPermission]
    return required.some((p) => userPermissions.includes(p))
  })

  if (visibleItems.length === 0) return null

  const hasToggle = Boolean(label) && Boolean(onToggle)

  // ── Collapsed + grouped → popup-menu (portaled flyout) ──
  if (collapsed && hasToggle && label) {
    return (
      <CollapsedGroupPopup
        items={visibleItems}
        label={label}
        onPopupCancelClose={onPopupCancelClose}
        onPopupClose={onPopupClose}
        onPopupHoverEnter={onPopupHoverEnter}
        onPopupHoverLeave={onPopupHoverLeave}
        onPopupToggle={onPopupToggle}
        popupOpen={popupOpen}
      />
    )
  }

  // ── Standard: expanded accordion OR standalone collapsed items ──
  const itemsVisible = !label || expanded

  return (
    <div className={cn('flex gap-1 lg:flex-col lg:gap-0.5', collapsed && 'lg:items-center')}>
      {/* Group header button — same height/style as standalone nav items */}
      {hasToggle && !collapsed && (
        <button
          aria-expanded={expanded}
          className={cn(
            'group/header hidden h-9 w-full shrink-0 cursor-pointer items-center gap-3 rounded-md px-3',
            'text-[13.5px] font-medium text-sidebar-fg transition-colors',
            'hover:bg-sidebar-hover-bg hover:text-gray-900',
            'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-gray-400/30',
            'lg:flex',
          )}
          onClick={onToggle}
          type="button"
        >
          {/* Icon from first visible item */}
          {(() => {
            const Icon = visibleItems[0].icon
            return (
              <Icon
                aria-hidden="true"
                className="shrink-0 text-gray-500 transition-colors group-hover/header:text-gray-900"
                size={17}
                strokeWidth={1.85}
              />
            )
          })()}
          <span className="flex-1 truncate text-left">{label}</span>
          <ChevronRight
            aria-hidden="true"
            className={cn(
              'shrink-0 text-gray-400 transition-transform duration-200 group-hover/header:text-gray-700',
              expanded && 'rotate-90',
            )}
            size={14}
            strokeWidth={2.25}
          />
        </button>
      )}

      {/* Items */}
      {itemsVisible && visibleItems.map((item) => (
        <NavLink
          className={({ isActive }) =>
            cn(
              'group relative flex h-9 shrink-0 items-center gap-3 rounded-md text-[13.5px] font-medium text-sidebar-fg transition-colors',
              'hover:bg-sidebar-hover-bg hover:text-gray-900',
              'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-gray-400/30',
              isActive && 'bg-sidebar-active-bg text-sidebar-active-fg font-semibold',
              // Sub-items (with group parent): indent to show hierarchy
              label ? 'px-3 pl-6' : 'px-3',
              collapsed && 'lg:h-9 lg:w-9 lg:justify-center lg:px-0 lg:pl-0',
            )
          }
          end={item.path === '/'}
          key={item.path}
          title={item.label}
          to={item.path}
        >
          {({ isActive }) => (
            <>
              {label ? (
                // Child item: subtle dot indicator
                <span className={cn('flex h-2 w-2 shrink-0 rounded-full', isActive ? 'bg-gray-500' : 'bg-gray-300')} />
              ) : (
                // Standalone item: full icon
                <item.icon
                  aria-hidden="true"
                  className={cn(
                    'shrink-0 transition-colors',
                    isActive ? 'text-gray-900' : 'text-gray-500 group-hover:text-gray-900',
                  )}
                  size={17}
                  strokeWidth={isActive ? 2.25 : 1.85}
                />
              )}
              <span className={cn('truncate', collapsed && 'lg:hidden')}>
                {item.label}
              </span>
            </>
          )}
        </NavLink>
      ))}
    </div>
  )
}

// ─── CollapsedGroupPopup ──────────────────────────────────────────────────────
// Portaled flyout for collapsed sidebar groups. Rendered into document.body
// via createPortal so it can't be clipped by the nav's overflow.

type CollapsedGroupPopupProps = {
  items: readonly NavigationItem[]
  label: string
  onPopupCancelClose?: () => void
  onPopupClose?: () => void
  onPopupHoverEnter?: () => void
  onPopupHoverLeave?: () => void
  onPopupToggle?: () => void
  popupOpen?: boolean
}

function CollapsedGroupPopup({
  items,
  label,
  onPopupCancelClose,
  onPopupClose,
  onPopupHoverEnter,
  onPopupHoverLeave,
  onPopupToggle,
  popupOpen,
}: Readonly<CollapsedGroupPopupProps>) {
  const triggerRef = useRef<HTMLButtonElement>(null)
  const [pos, setPos] = useState<{ top: number; left: number } | null>(null)

  useEffect(() => {
    if (!popupOpen || !triggerRef.current) {
      setPos(null)
      return
    }
    function place() {
      const el = triggerRef.current
      if (!el) return
      const rect = el.getBoundingClientRect()
      setPos({ top: rect.top, left: rect.right + 8 })
    }
    place()
    window.addEventListener('resize', place)
    return () => window.removeEventListener('resize', place)
  }, [popupOpen])

  return (
    <div className="hidden lg:block" data-sidebar-popup>
      <button
        aria-expanded={popupOpen}
        aria-haspopup="menu"
        aria-label={label}
        className={cn(
          'flex h-9 w-9 cursor-pointer items-center justify-center rounded-md transition-colors',
          popupOpen
            ? 'bg-sidebar-hover-bg text-gray-900'
            : 'text-gray-500 hover:bg-sidebar-hover-bg hover:text-gray-700',
        )}
        onClick={onPopupToggle}
        onMouseEnter={onPopupHoverEnter}
        onMouseLeave={onPopupHoverLeave}
        ref={triggerRef}
        title={label}
        type="button"
      >
        {(() => {
          const Icon = items[0].icon
          return <Icon aria-hidden="true" size={18} strokeWidth={1.85} />
        })()}
      </button>

      {popupOpen && pos && createPortal(
        <div
          aria-label={label}
          className="fixed z-50 w-60 overflow-hidden rounded-xl border border-gray-200 bg-white py-1 shadow-lg ring-1 ring-black/5 animate-in fade-in slide-in-from-left-1 duration-150 ease-out"
          data-sidebar-popup
          onMouseEnter={onPopupCancelClose}
          onMouseLeave={onPopupHoverLeave}
          role="menu"
          style={{ left: pos.left, top: pos.top }}
        >
          <div className="border-b border-gray-100 px-3 py-2">
            <p className="text-[10px] font-semibold uppercase tracking-[0.12em] text-gray-500">
              {label}
            </p>
          </div>
          <div className="py-1">
            {items.map((item) => (
              <NavLink
                className={({ isActive }) =>
                  cn(
                    'flex items-center gap-3 px-3 py-2 text-sm font-medium transition-colors',
                    isActive
                      ? 'bg-gray-100 text-gray-900'
                      : 'text-gray-700 hover:bg-gray-100 hover:text-gray-900',
                  )
                }
                end={item.path === '/'}
                key={item.path}
                onClick={onPopupClose}
                role="menuitem"
                to={item.path}
              >
                <span className="flex h-2 w-2 shrink-0 rounded-full bg-gray-300" />
                <span className="truncate">{item.label}</span>
              </NavLink>
            ))}
          </div>
        </div>,
        document.body,
      )}
    </div>
  )
}
