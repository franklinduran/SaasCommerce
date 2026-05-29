import { Bell, Settings, PanelLeftClose, PanelLeftOpen } from 'lucide-react'
import { Fragment, useEffect, useRef, useState } from 'react'
import { Link, NavLink } from 'react-router-dom'
import bimmoLogo from '@/assets/bimmo/bimmo.svg'
import bimmoFavicon from '@/assets/bimmo/favicon.svg'
import { NavigationSection } from '@/app/components/NavigationSection'
import { SidebarUserMenu } from '@/app/components/SidebarUserMenu'
import { navigationGroups, SIDEBAR_EXPANDED_GROUP_KEY, loadExpandedGroup } from '@/app/navigation'
import { useUnreadNotificationCount } from '@/modules/notifications/hooks/useNotifications'
import { useAppStore } from '@/shared/hooks/useAppStore'
import { cn } from '@/shared/utils/cn'

type AppSidebarProps = {
  userPermissions: string[]
}

/**
 * Main application sidebar.
 * - Expanded (260px): logo + accordion nav + settings link + user footer
 * - Collapsed (76px): favicon + icon-only nav + popup flyouts + user footer
 */
export function AppSidebar({ userPermissions }: Readonly<AppSidebarProps>) {
  const sidebarCollapsed = useAppStore((state) => state.sidebarCollapsed)
  const toggleSidebar = useAppStore((state) => state.toggleSidebar)
  const { data: notifData } = useUnreadNotificationCount()
  const unreadCount = notifData?.unreadCount ?? 0

  // Accordion: only ONE group open at a time. Persisted to localStorage.
  const [expandedGroup, setExpandedGroup] = useState<string | null>(loadExpandedGroup)

  // Auto-expand group containing the active route
  const { pathname } = window.location
  useEffect(() => {
    const activeGroup = navigationGroups.find((g) =>
      g.items.some((item) => item.path === pathname),
    )
    if (activeGroup?.label && expandedGroup !== activeGroup.label) {
      setExpandedGroup(activeGroup.label)
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pathname])

  useEffect(() => {
    try {
      localStorage.setItem(SIDEBAR_EXPANDED_GROUP_KEY, expandedGroup ?? '')
    } catch { /* ignore quota / privacy mode errors */ }
  }, [expandedGroup])

  function toggleGroup(label: string) {
    setExpandedGroup((curr) => (curr === label ? null : label))
  }

  // Collapsed-mode popup state + grace-period timer
  const [openPopup, setOpenPopup] = useState<string | null>(null)
  const popupCloseTimerRef = useRef<number | null>(null)

  function cancelPopupClose() {
    if (popupCloseTimerRef.current !== null) {
      clearTimeout(popupCloseTimerRef.current)
      popupCloseTimerRef.current = null
    }
  }

  function openPopupHover(label: string) {
    cancelPopupClose()
    setOpenPopup(label)
  }

  function schedulePopupClose(label: string) {
    cancelPopupClose()
    popupCloseTimerRef.current = window.setTimeout(() => {
      setOpenPopup((curr) => (curr === label ? null : curr))
      popupCloseTimerRef.current = null
    }, 200)
  }

  function closePopupNow() {
    cancelPopupClose()
    setOpenPopup(null)
  }

  useEffect(() => () => cancelPopupClose(), [])

  // Close popup on outside click or Escape
  useEffect(() => {
    if (!openPopup) return
    function handlePointerDown(event: MouseEvent) {
      if (!(event.target as Element | null)?.closest('[data-sidebar-popup]')) closePopupNow()
    }
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') closePopupNow()
    }
    document.addEventListener('mousedown', handlePointerDown)
    document.addEventListener('keydown', handleKeyDown)
    return () => {
      document.removeEventListener('mousedown', handlePointerDown)
      document.removeEventListener('keydown', handleKeyDown)
    }
  }, [openPopup])

  // Close popups when sidebar expands/collapses
  useEffect(() => { closePopupNow() }, [sidebarCollapsed])

  return (
    <aside
      className={cn(
        'relative flex w-full min-w-0 max-w-full shrink-0 flex-col border-b border-sidebar-border bg-sidebar-bg text-sidebar-fg',
        'lg:row-span-2 lg:max-h-dvh lg:min-h-0 lg:border-b-0 lg:border-r',
        sidebarCollapsed ? 'lg:w-[76px]' : 'lg:w-[260px]',
      )}
    >
      {/* ── Logo header ──────────────────────────────────────────────────── */}
      <div
        className={cn(
          'flex h-16 shrink-0 items-center border-b border-sidebar-border/60',
          sidebarCollapsed ? 'justify-center px-3' : 'px-4',
        )}
      >
        <Link
          className="shrink-0 cursor-pointer rounded-md focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-gray-400/30"
          title="Inicio"
          to="/"
        >
          <img
            alt="Bimmo"
            className={cn('object-contain', sidebarCollapsed ? 'h-8 w-8' : 'h-9 w-auto')}
            src={sidebarCollapsed ? bimmoFavicon : bimmoLogo}
          />
        </Link>

        {!sidebarCollapsed && (
          <>
            <div className="flex-1" />
            <button
              aria-label="Contraer menu"
              className="flex h-9 w-9 shrink-0 cursor-pointer items-center justify-center rounded-md text-gray-500 transition-colors hover:bg-sidebar-hover-bg hover:text-gray-900 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-gray-400/30 max-lg:hidden lg:inline-flex"
              onClick={toggleSidebar}
              title="Contraer menu"
              type="button"
            >
              <PanelLeftClose aria-hidden="true" size={17} strokeWidth={2} />
            </button>
          </>
        )}
      </div>

      {/* Floating expand button (collapsed mode only) */}
      {sidebarCollapsed && (
        <button
          aria-label="Expandir menu"
          className="absolute right-0 top-16 z-30 hidden h-6 w-6 -translate-y-1/2 translate-x-1/2 cursor-pointer items-center justify-center rounded-full border border-gray-200 bg-white text-gray-500 shadow-sm transition-all hover:border-gray-300 hover:text-gray-900 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-gray-400/30 lg:flex"
          onClick={toggleSidebar}
          title="Expandir menu"
          type="button"
        >
          <PanelLeftOpen aria-hidden="true" size={12} strokeWidth={2.5} />
        </button>
      )}

      {/* ── Navigation ───────────────────────────────────────────────────── */}
      <nav
        className={cn(
          'flex min-w-0 shrink-0 gap-1 overflow-x-auto px-3 py-2',
          'lg:min-h-0 lg:flex-1 lg:flex-col lg:gap-3 lg:overflow-y-auto lg:overflow-x-hidden lg:px-3 lg:py-3',
          sidebarCollapsed && 'lg:items-center',
        )}
      >
        {navigationGroups.map((group, i) => (
          <Fragment key={group.label ?? `group-${i}`}>
            {sidebarCollapsed && i > 0 && (
              <hr aria-hidden="true" className="hidden h-px w-6 border-0 bg-gray-200 lg:block" />
            )}
            <NavigationSection
              collapsed={sidebarCollapsed}
              expanded={group.label ? expandedGroup === group.label : true}
              items={group.items}
              label={group.label}
              onPopupCancelClose={cancelPopupClose}
              onPopupClose={closePopupNow}
              onPopupHoverEnter={group.label ? () => openPopupHover(group.label!) : undefined}
              onPopupHoverLeave={group.label ? () => schedulePopupClose(group.label!) : undefined}
              onPopupToggle={group.label ? () => (openPopup === group.label ? closePopupNow() : openPopupHover(group.label!)) : undefined}
              onToggle={group.label ? () => toggleGroup(group.label!) : undefined}
              popupOpen={group.label !== undefined && openPopup === group.label}
              userPermissions={userPermissions}
            />
          </Fragment>
        ))}

        {/* Notificaciones — standalone nav item with unread badge */}
        <NavLink
          className={({ isActive }) =>
            cn(
              'group relative flex h-9 shrink-0 items-center gap-3 rounded-md px-3 text-[13.5px] font-medium text-sidebar-fg transition-colors',
              'hover:bg-sidebar-hover-bg hover:text-gray-900',
              'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-gray-400/30',
              isActive && 'bg-sidebar-active-bg text-sidebar-active-fg font-semibold',
              sidebarCollapsed && 'lg:h-9 lg:w-9 lg:justify-center lg:px-0',
            )
          }
          title="Notificaciones"
          to="/notifications"
        >
          {({ isActive }) => (
            <>
              <span className="relative shrink-0">
                <Bell
                  aria-hidden="true"
                  className={cn('transition-colors', isActive ? 'text-gray-900' : 'text-gray-500 group-hover:text-gray-900')}
                  size={17}
                  strokeWidth={isActive ? 2.25 : 1.85}
                />
                {unreadCount > 0 && (
                  <span
                    aria-hidden="true"
                    className="absolute -right-1 -top-1 flex h-3.5 min-w-3.5 items-center justify-center rounded-full bg-red-500 px-0.5 text-[8px] font-bold leading-none text-white"
                  >
                    {unreadCount > 99 ? '99+' : unreadCount}
                  </span>
                )}
              </span>
              {!sidebarCollapsed && (
                <span className="flex-1 truncate">Notificaciones</span>
              )}
              {!sidebarCollapsed && unreadCount > 0 && (
                <span className="shrink-0 rounded-full bg-red-100 px-1.5 py-0.5 text-[10px] font-semibold leading-none text-red-600">
                  {unreadCount > 99 ? '99+' : unreadCount}
                </span>
              )}
            </>
          )}
        </NavLink>

        {/* Ajustes — standalone nav item after Sistema, same style as Inicio */}
        <NavLink
          className={({ isActive }) =>
            cn(
              'group relative flex h-9 shrink-0 items-center gap-3 rounded-md px-3 text-[13.5px] font-medium text-sidebar-fg transition-colors',
              'hover:bg-sidebar-hover-bg hover:text-gray-900',
              'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-gray-400/30',
              isActive && 'bg-sidebar-active-bg text-sidebar-active-fg font-semibold',
              sidebarCollapsed && 'lg:h-9 lg:w-9 lg:justify-center lg:px-0',
            )
          }
          title="Ajustes"
          to="/settings"
        >
          {({ isActive }) => (
            <>
              <Settings
                aria-hidden="true"
                className={cn('shrink-0 transition-colors', isActive ? 'text-gray-900' : 'text-gray-500 group-hover:text-gray-900')}
                size={17}
                strokeWidth={isActive ? 2.25 : 1.85}
              />
              {!sidebarCollapsed && <span>Ajustes</span>}
            </>
          )}
        </NavLink>
      </nav>

      {/* ── User footer ──────────────────────────────────────────────────── */}
      <SidebarUserMenu collapsed={sidebarCollapsed} userPermissions={userPermissions} />
    </aside>
  )
}
