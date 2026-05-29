import { ChevronRight, Clock, Search } from 'lucide-react'
import { Fragment } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { NotificationBell } from '@/modules/notifications/components/NotificationBell'

export type Breadcrumb = {
  label: string
  path: string
}

type AppTopbarProps = {
  breadcrumbs: Breadcrumb[]
  onSearchOpen: () => void
}

/**
 * Application topbar: breadcrumbs on the left, search + actions on the right.
 * Height matches the sidebar logo header (h-16 / 64px).
 */
export function AppTopbar({ breadcrumbs, onSearchOpen }: Readonly<AppTopbarProps>) {
  const navigate = useNavigate()
  const isLast = (i: number) => i === breadcrumbs.length - 1

  return (
    <header className="sticky top-0 z-20 hidden h-16 shrink-0 items-center justify-between border-b border-sidebar-border bg-sidebar-bg px-6 lg:flex">

      {/* Left: breadcrumb trail — all entries are links except the current page */}
      <nav
        aria-label="Breadcrumb"
        className="flex min-w-0 items-center gap-1 text-[11px] font-semibold uppercase tracking-[0.1em]"
      >
        {breadcrumbs.map((crumb, i) => (
          <Fragment key={crumb.path}>
            {i > 0 && (
              <ChevronRight aria-hidden="true" className="shrink-0 text-gray-300" size={9} strokeWidth={2.5} />
            )}
            {isLast(i) ? (
              <span className="truncate text-gray-700" aria-current="page">{crumb.label}</span>
            ) : (
              <Link
                className="truncate text-gray-400 transition-colors hover:text-gray-700 focus-visible:outline-none focus-visible:underline"
                to={crumb.path}
              >
                {crumb.label}
              </Link>
            )}
          </Fragment>
        ))}
      </nav>

      {/* Right: search bar + audit shortcut + notifications */}
      <div className="flex shrink-0 items-center gap-1">
        {/* Search trigger — compact, right-aligned with other actions */}
        <button
          aria-label="Búsqueda"
          className="flex h-9 w-56 items-center gap-2 rounded-lg border border-gray-200 bg-white px-3 text-gray-400 transition-colors hover:border-gray-300 hover:text-gray-500 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-gray-400/30"
          onClick={onSearchOpen}
          title="Buscar (Ctrl+K)"
          type="button"
        >
          <Search aria-hidden="true" className="shrink-0" size={14} strokeWidth={1.85} />
          <span className="flex-1 text-left text-[12px] font-normal">Buscar...</span>
          <kbd className="shrink-0 rounded bg-gray-100 px-1.5 py-0.5 text-[10px] font-medium text-gray-400">
            ⌘K
          </kbd>
        </button>

        {/* Audit log shortcut */}
        <button
          aria-label="Auditoría"
          className="flex h-9 w-9 cursor-pointer items-center justify-center rounded-md text-gray-500 transition-colors hover:bg-gray-100 hover:text-gray-900 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-gray-400/30"
          onClick={() => navigate('/audit-logs')}
          title="Auditoría"
          type="button"
        >
          <Clock aria-hidden="true" size={17} strokeWidth={1.85} />
        </button>

        <NotificationBell />
      </div>
    </header>
  )
}
