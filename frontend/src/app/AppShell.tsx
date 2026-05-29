import { useEffect, useMemo, useState } from 'react'
import { Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useAuthStore } from '@/modules/auth/authStore'
import { SubscriptionAlertBanner } from '@/modules/subscription/components/SubscriptionAlertBanner'
import { AppSidebar } from '@/app/components/AppSidebar'
import { AppTopbar, type Breadcrumb } from '@/app/components/AppTopbar'
import { CommandPalette, type CommandPaletteItem } from '@/app/CommandPalette'
import {
  BANNER_DISMISSED_KEY,
  getPageTitle,
  navigationGroups,
} from '@/app/navigation'
import { useCurrentUserPermissions } from '@/shared/hooks/usePermissions'

/**
 * Root application shell: sidebar + topbar + main content area.
 *
 * Orchestrates:
 * - AppSidebar (navigation, user footer)
 * - AppTopbar (breadcrumbs, search, quick actions)
 * - CommandPalette (Ctrl/Cmd+K spotlight search)
 * - SubscriptionAlertBanner (dismissable, session-scoped)
 * - Redirect to /change-password when required
 */
export function AppShell() {
  const session = useAuthStore((state) => state.session)
  const { data: permissionsData } = useCurrentUserPermissions()
  const userPermissions = permissionsData?.permissions ?? []
  const location = useLocation()
  const navigate = useNavigate()

  // ── Breadcrumbs ─────────────────────────────────────────────────────────
  const pageTitle = getPageTitle(location.pathname)
  const breadcrumbs = useMemo<Breadcrumb[]>(
    () =>
      location.pathname === '/'
        ? [{ label: 'Inicio', path: '/' }]
        : [{ label: 'Inicio', path: '/' }, { label: pageTitle, path: location.pathname }],
    [location.pathname, pageTitle],
  )

  // ── Subscription banner — session-scoped (cleared on logout) ────────────
  const [bannerDismissed, setBannerDismissed] = useState<boolean>(
    () => sessionStorage.getItem(BANNER_DISMISSED_KEY) === 'true',
  )

  function dismissBanner() {
    sessionStorage.setItem(BANNER_DISMISSED_KEY, 'true')
    setBannerDismissed(true)
  }

  // ── Command palette ──────────────────────────────────────────────────────
  const [searchOpen, setSearchOpen] = useState(false)

  const commandItems = useMemo<CommandPaletteItem[]>(
    () =>
      navigationGroups.flatMap((g) =>
        g.items.map((item) => ({
          groupLabel: g.label,
          icon: item.icon,
          label: item.label,
          path: item.path,
          requiredPermission: item.requiredPermission,
        })),
      ),
    [],
  )

  // Global Ctrl/Cmd+K shortcut
  useEffect(() => {
    function handleKeyDown(event: KeyboardEvent) {
      if ((event.metaKey || event.ctrlKey) && event.key.toLowerCase() === 'k') {
        event.preventDefault()
        setSearchOpen((open) => !open)
      }
    }
    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [])

  // ── Force password change ────────────────────────────────────────────────
  useEffect(() => {
    if (session?.user.mustChangePassword && location.pathname !== '/change-password') {
      navigate('/change-password', { replace: true })
    }
  }, [session?.user.mustChangePassword, navigate, location.pathname])

  const isHome = location.pathname === '/'

  return (
    <div className="h-dvh overflow-hidden bg-background text-foreground">
      <div className="grid h-full w-full grid-rows-[auto_minmax(0,1fr)] overflow-hidden bg-background lg:grid-cols-[auto_minmax(0,1fr)] lg:grid-rows-[64px_minmax(0,1fr)]">

        {/* Sidebar — spans both grid rows on lg+ */}
        <AppSidebar userPermissions={userPermissions} />

        {/* Topbar */}
        <AppTopbar breadcrumbs={breadcrumbs} onSearchOpen={() => setSearchOpen(true)} />

        {/* Main content */}
        <main className="min-h-0 min-w-0 overflow-y-auto overflow-x-hidden bg-background">
          {isHome && !bannerDismissed && (
            <SubscriptionAlertBanner
              className="sticky top-0 z-10 rounded-none border-x-0 border-t-0"
              onChoosePlan={() => navigate('/subscription')}
              onReactivateClick={() => navigate('/subscription')}
              onContactSupport={() => {
                window.location.href = 'mailto:soporte@bimmo.app?subject=Soporte%20suscripcion'
              }}
              onDismiss={dismissBanner}
            />
          )}
          <Outlet />
        </main>
      </div>

      {/* Command palette — portaled, mounts on demand */}
      {searchOpen && (
        <CommandPalette
          items={commandItems}
          onClose={() => setSearchOpen(false)}
          userPermissions={userPermissions}
        />
      )}
    </div>
  )
}
