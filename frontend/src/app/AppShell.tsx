import {
  ArrowLeftRight,
  Banknote,
  BarChart2,
  BarChart3,
  Boxes,
  Building2,
  CalendarCheck,
  ChevronDown,
  ChevronRight,
  CircleDollarSign,
  ClipboardList,
  CreditCard,
  GitBranch,
  History,
  LayoutDashboard,
  LogOut,
  MessageSquareWarning,
  Package,
  PanelLeftClose,
  PanelLeftOpen,
  ReceiptText,
  Search,
  Settings,
  Shield,
  ShoppingCart,
  TrendingUp,
  Truck,
  Users,
  Wallet,
} from 'lucide-react'
import type { LucideIcon } from 'lucide-react'
import { Fragment, useEffect, useMemo, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import { CommandPalette, type CommandPaletteItem } from '@/app/CommandPalette'
import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useAuthStore } from '@/modules/auth/authStore'
import { logout as serverLogout } from '@/modules/auth/services/authService'
import { NotificationBell } from '@/modules/notifications/components/NotificationBell'
import { SubscriptionAlertBanner } from '@/modules/subscription/components/SubscriptionAlertBanner'
import { Button } from '@/shared/components/ui/button'
import { useAppStore } from '@/shared/hooks/useAppStore'
import { useCurrentUserPermissions } from '@/shared/hooks/usePermissions'
import { Permission } from '@/shared/types/permissions'
import type { PermissionCode } from '@/shared/types/permissions'
import { cn } from '@/shared/utils/cn'

type NavigationItem = {
  label: string
  path: string
  icon: LucideIcon
  /** Permission(s) required to see this item. Undefined = always visible. */
  requiredPermission?: PermissionCode | PermissionCode[]
}

type NavigationGroup = {
  /** Optional section heading. Omit for the standalone item at top (e.g. Inicio). */
  label?: string
  items: readonly NavigationItem[]
}

/**
 * Navigation organized into compact, scannable groups.
 * Each group surfaces 2-5 related actions. Order = priority of use during
 * the daily workflow of a colmado / retail business.
 */
const navigationGroups: readonly NavigationGroup[] = [
  // Standalone — the dashboard always sits at the top.
  {
    items: [
      { label: 'Inicio', path: '/', icon: LayoutDashboard, requiredPermission: Permission.DashboardView },
    ],
  },
  {
    label: 'Ventas',
    items: [
      { label: 'POS', path: '/pos', icon: ShoppingCart, requiredPermission: Permission.SalesCreate },
      { label: 'Ventas', path: '/sales', icon: History, requiredPermission: Permission.SalesView },
      { label: 'Caja', path: '/cash', icon: Wallet, requiredPermission: Permission.CashView },
      { label: 'Arqueo de caja', path: '/cash-register', icon: Banknote, requiredPermission: Permission.CashView },
      { label: 'Cierre diario', path: '/daily-closing', icon: CalendarCheck, requiredPermission: Permission.DailyClosingView },
    ],
  },
  {
    label: 'Inventario',
    items: [
      { label: 'Productos', path: '/products', icon: Package, requiredPermission: Permission.ProductsView },
      { label: 'Stock', path: '/inventory', icon: Boxes, requiredPermission: Permission.InventoryView },
      { label: 'Transferencias', path: '/inventory-transfers', icon: ArrowLeftRight, requiredPermission: Permission.InventoryTransfer },
    ],
  },
  {
    label: 'Finanzas',
    items: [
      { label: 'Compras', path: '/purchases', icon: Truck, requiredPermission: Permission.PurchasesView },
      { label: 'Gastos', path: '/expenses', icon: CircleDollarSign, requiredPermission: Permission.ExpensesView },
      { label: 'Recibos', path: '/invoices', icon: ReceiptText, requiredPermission: Permission.InvoicesView },
    ],
  },
  {
    label: 'Contactos',
    items: [
      { label: 'Clientes', path: '/customers', icon: Users, requiredPermission: Permission.CustomersView },
      { label: 'Proveedores', path: '/suppliers', icon: Building2, requiredPermission: Permission.PurchasesView },
      { label: 'Sucursales', path: '/branches', icon: GitBranch, requiredPermission: Permission.BranchesView },
    ],
  },
  {
    label: 'Análisis',
    items: [
      { label: 'Reportes', path: '/reports', icon: BarChart3, requiredPermission: Permission.ReportsView },
      { label: 'Rentabilidad', path: '/profitability', icon: TrendingUp, requiredPermission: Permission.ProfitabilityView },
    ],
  },
  {
    label: 'Sistema',
    items: [
      { label: 'Usuarios', path: '/users', icon: Shield, requiredPermission: Permission.UsersView },
      { label: 'Auditoría', path: '/audit-logs', icon: ClipboardList, requiredPermission: Permission.AuditView },
      { label: 'Suscripción', path: '/subscription', icon: CreditCard },
      { label: 'Ajustes', path: '/settings', icon: Settings },
    ],
  },
  {
    label: 'Plataforma',
    items: [
      { label: 'Feedback Beta', path: '/beta-feedback', icon: MessageSquareWarning, requiredPermission: Permission.BetaFeedbackView },
      { label: 'Métricas piloto', path: '/admin/pilot-metrics', icon: BarChart2, requiredPermission: Permission.SaasPilotMetrics },
    ],
  },
]

const pageTitles: Record<string, string> = {
  '/': 'Inicio',
  '/pos': 'POS',
  '/cash': 'Caja',
  '/cash/history': 'Historial de cajas',
  '/cash-register': 'Arqueo de caja avanzado',
  '/cash-register/history': 'Historial de arqueos',
  '/cash-register/daily-summary': 'Arqueo diario',
  '/daily-closing': 'Cierre diario',
  '/daily-closing/history': 'Historial de cierres',
  '/notifications': 'Notificaciones',
  '/expenses': 'Gastos operativos',
  '/expenses/new': 'Nuevo gasto',
  '/expenses/categories': 'Categorías de gastos',
  '/sales': 'Ventas',
  '/products': 'Productos',
  '/inventory': 'Inventario',
  '/inventory-transfers': 'Transferencias',
  '/branches': 'Sucursales',
  '/customers': 'Clientes',
  '/suppliers': 'Proveedores',
  '/purchases': 'Compras',
  '/invoices': 'Recibos',
  '/reports': 'Reportes',
  '/profitability': 'Rentabilidad',
  '/profitability/products': 'Rentabilidad por producto',
  '/profitability/branches': 'Rentabilidad por sucursal',
  '/profitability/alerts': 'Alertas de rentabilidad',
  '/users': 'Usuarios',
  '/audit-logs': 'Auditoria',
  '/subscription': 'Mi Suscripcion',
  '/beta-feedback': 'Feedback Beta',
  '/admin/pilot-metrics': 'Metricas del Piloto',
  '/settings': 'Ajustes',
  '/forbidden': 'Acceso denegado',
}

const BANNER_DISMISSED_KEY = 'subscription-banner-dismissed'
const SIDEBAR_EXPANDED_GROUP_KEY = 'sidebar-expanded-group-v2'

function loadExpandedGroup(): string | null {
  // Default for first-time users: open the first named group (most-used flow).
  const firstGroup = navigationGroups.find((g) => g.label)?.label ?? null
  try {
    const stored = localStorage.getItem(SIDEBAR_EXPANDED_GROUP_KEY)
    // Never stored → use default. Empty string → user explicitly collapsed all.
    if (stored === null) return firstGroup
    return stored || null
  } catch {
    return firstGroup
  }
}

export function AppShell() {
  const businessName = useAppStore((state) => state.businessName)
  const sidebarCollapsed = useAppStore((state) => state.sidebarCollapsed)
  const toggleSidebar = useAppStore((state) => state.toggleSidebar)
  const session = useAuthStore((state) => state.session)
  const clearSession = useAuthStore((state) => state.clearSession)
  const { data: permissionsData } = useCurrentUserPermissions()
  const userPermissions = permissionsData?.permissions ?? []
  const location = useLocation()
  const navigate = useNavigate()
  const pageTitle = getPageTitle(location.pathname)
  const [userMenuOpen, setUserMenuOpen] = useState(false)
  const userMenuRef = useRef<HTMLDivElement>(null)
  const userName = session?.user.fullName ?? 'Admin'
  const userInitial = userName.trim().slice(0, 1).toUpperCase() || 'A'
  const userRoleLabel = formatRoleLabel(session?.user.roles?.[0])
  const canManageUsers = userPermissions.includes(Permission.UsersView)

  // Banner descartable — se resetea al cerrar sesión (sessionStorage)
  const [bannerDismissed, setBannerDismissed] = useState<boolean>(
    () => sessionStorage.getItem(BANNER_DISMISSED_KEY) === 'true'
  )

  // Sidebar — only ONE group can be expanded at a time (accordion behavior).
  // Persisted to localStorage. `null` = all collapsed.
  const [expandedGroup, setExpandedGroup] = useState<string | null>(loadExpandedGroup)

  // Auto-expand the group containing the currently active route, so the
  // user always sees where they are in the nav tree.
  useEffect(() => {
    const activeGroup = navigationGroups.find((g) =>
      g.items.some((item) => item.path === location.pathname),
    )
    if (activeGroup?.label && expandedGroup !== activeGroup.label) {
      setExpandedGroup(activeGroup.label)
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [location.pathname])

  // Persist whenever user toggles. Empty string = user explicitly closed all
  // (different from "never stored" which falls back to the default group).
  useEffect(() => {
    try {
      localStorage.setItem(SIDEBAR_EXPANDED_GROUP_KEY, expandedGroup ?? '')
    } catch {
      // ignore quota / privacy mode errors
    }
  }, [expandedGroup])

  function toggleGroup(label: string) {
    // Click the open group → close it. Click another → it becomes the only open.
    setExpandedGroup((curr) => (curr === label ? null : label))
  }

  // Collapsed-mode popup: which group's flyout menu is open. Only one at a time.
  const [openPopup, setOpenPopup] = useState<string | null>(null)
  // Grace-period timer for hover-out: prevents the popup from snapping shut
  // when the user moves their mouse from the trigger to the popup.
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
      // Only close if THIS popup is still the open one — guards against the
      // race where the user moves from one group to another within the delay.
      setOpenPopup((curr) => (curr === label ? null : curr))
      popupCloseTimerRef.current = null
    }, 200)
  }

  function closePopupNow() {
    cancelPopupClose()
    setOpenPopup(null)
  }

  // Cleanup timer on unmount
  useEffect(() => () => cancelPopupClose(), [])

  // Close popup on outside click or escape (matches user-menu pattern).
  useEffect(() => {
    if (!openPopup) return
    function handlePointerDown(event: MouseEvent) {
      if (!(event.target as Element | null)?.closest('[data-sidebar-popup]')) {
        closePopupNow()
      }
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

  // Close any open popup when toggling between collapsed/expanded modes.
  useEffect(() => {
    closePopupNow()
  }, [sidebarCollapsed])

  // ─── Command palette (Ctrl/Cmd+K search) ─────────────────────────────────
  const [searchOpen, setSearchOpen] = useState(false)

  // Flatten all nav items so the search can match across every group.
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

  // Global Ctrl/Cmd+K to toggle the palette.
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

  function dismissBanner() {
    sessionStorage.setItem(BANNER_DISMISSED_KEY, 'true')
    setBannerDismissed(true)
  }

  const isHome = location.pathname === '/'

  // Redirigir a /change-password si el usuario debe cambiar contraseña
  useEffect(() => {
    if (session?.user.mustChangePassword && location.pathname !== '/change-password') {
      navigate('/change-password', { replace: true })
    }
  }, [session?.user.mustChangePassword, navigate, location.pathname])

  useEffect(() => {
    if (!userMenuOpen) return

    function handlePointerDown(event: MouseEvent) {
      if (!userMenuRef.current?.contains(event.target as Node)) {
        setUserMenuOpen(false)
      }
    }

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') {
        setUserMenuOpen(false)
      }
    }

    document.addEventListener('mousedown', handlePointerDown)
    document.addEventListener('keydown', handleKeyDown)
    return () => {
      document.removeEventListener('mousedown', handlePointerDown)
      document.removeEventListener('keydown', handleKeyDown)
    }
  }, [userMenuOpen])

  function navigateFromUserMenu(path: string) {
    setUserMenuOpen(false)
    navigate(path)
  }

  async function handleLogout() {
    setUserMenuOpen(false)
    if (session) {
      try {
        await serverLogout(session.accessToken, session.refreshToken)
      } catch {
        // Ignore server errors — still clear the local session
      }
    }
    clearSession()
    navigate('/login', { replace: true })
  }

  return (
    <div className="h-dvh overflow-hidden bg-background text-foreground">
      <div className="grid h-full w-full grid-rows-[auto_minmax(0,1fr)] overflow-hidden bg-background lg:grid-cols-[auto_minmax(0,1fr)] lg:grid-rows-[64px_minmax(0,1fr)]">
        {/* ── Sidebar ────────────────────────────────────────────────────── */}
        <aside
          className={cn(
            'relative flex w-full min-w-0 max-w-full shrink-0 flex-col border-b border-sidebar-border bg-sidebar-bg text-sidebar-fg lg:row-span-2 lg:max-h-dvh lg:min-h-0 lg:border-b-0 lg:border-r',
            sidebarCollapsed ? 'lg:w-[76px]' : 'lg:w-[260px]',
          )}
        >
          {/* Logo / business header — logo always visible, even in collapsed mode */}
          <div
            className={cn(
              'flex h-16 shrink-0 items-center border-b border-sidebar-border/60',
              sidebarCollapsed ? 'justify-center px-3' : 'gap-2.5 px-3',
            )}
          >
            <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-md bg-sidebar-active-bg text-sidebar-active-fg shadow-sm">
              <CircleDollarSign aria-hidden="true" size={18} />
            </span>
            {!sidebarCollapsed && (
              <>
                <div className="min-w-0 flex-1">
                  <p className="truncate text-sm font-semibold text-sidebar-fg">{businessName}</p>
                  <p className="truncate text-xs font-medium text-sidebar-muted">
                    {session?.user.fullName ?? 'Sucursal principal'}
                  </p>
                </div>
                {/* Inline collapse button — next to the logo when expanded */}
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

          {/* Floating expand button — ONLY shown when collapsed, sits on the
              right edge of the sidebar, vertically centered on the divider. */}
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

          {/* Search bar (expanded only) — opens the command palette */}
          {!sidebarCollapsed && (
            <div className="hidden px-3 py-3 lg:block">
              <button
                aria-label="Buscar"
                className="flex w-full cursor-pointer items-center gap-2 rounded-md bg-sidebar-hover-bg px-3 py-2 text-sidebar-fg ring-1 ring-sidebar-border/60 transition-colors hover:bg-gray-100 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-gray-400/30"
                onClick={() => setSearchOpen(true)}
                type="button"
              >
                <Search aria-hidden="true" className="text-gray-500" size={15} />
                <span className="text-sm font-medium text-gray-500">Buscar...</span>
                <kbd className="ml-auto rounded bg-white px-1.5 py-0.5 font-mono text-[10px] font-semibold text-gray-500 ring-1 ring-gray-200">
                  Ctrl K
                </kbd>
              </button>
            </div>
          )}

          {/* Navigation — px-3 / py-3 / gap-3 consistent both modes */}
          <nav
            className={cn(
              'flex min-w-0 shrink-0 gap-1 overflow-x-auto px-3 py-2 lg:min-h-0 lg:flex-1 lg:flex-col lg:gap-3 lg:overflow-y-auto lg:overflow-x-hidden lg:px-3 lg:py-3',
              sidebarCollapsed && 'lg:items-center',
            )}
          >
            {navigationGroups.map((group, i) => (
              <Fragment key={group.label ?? `group-${i}`}>
                {/* Section separator in collapsed mode — visually splits one
                    group from the next so users can tell where each starts. */}
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
          </nav>

          {/* User menu trigger — px-3 py-3 consistent with nav */}
          <div className="hidden shrink-0 border-t border-sidebar-border/60 px-3 py-3 lg:block">
            <div className="relative" ref={userMenuRef}>
              <button
                aria-expanded={userMenuOpen}
                aria-haspopup="menu"
                aria-label="Abrir menu de usuario"
                className={cn(
                  'flex w-full cursor-pointer items-center gap-2.5 rounded-md px-2 py-1.5 text-left transition-colors hover:bg-sidebar-hover-bg focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-gray-400/30',
                  sidebarCollapsed && 'h-9 justify-center px-0',
                )}
                onClick={() => setUserMenuOpen((open) => !open)}
                type="button"
              >
                <div className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-sidebar-active-bg text-[11px] font-semibold text-sidebar-active-fg">
                  {userInitial}
                </div>
                <div className={cn('min-w-0 flex-1', sidebarCollapsed && 'lg:hidden')}>
                  <p className="truncate text-sm font-semibold text-sidebar-fg">
                    {userName}
                  </p>
                  <p className="truncate text-xs font-medium text-sidebar-muted">{userRoleLabel}</p>
                </div>
                <ChevronDown
                  aria-hidden="true"
                  className={cn(
                    'text-sidebar-muted transition-transform',
                    userMenuOpen && 'rotate-180',
                    sidebarCollapsed && 'lg:hidden',
                  )}
                  size={14}
                />
              </button>

              {userMenuOpen && (
                <div
                  aria-label="Menu de usuario"
                  className="absolute bottom-full left-0 z-40 mb-2 w-64 overflow-hidden rounded-xl border border-border bg-card py-1 shadow-lg ring-1 ring-black/5"
                  role="menu"
                >
                  <div className="border-b border-muted px-3 py-2">
                    <p className="truncate text-sm font-semibold text-foreground">{userName}</p>
                    <p className="truncate text-xs font-medium text-muted-foreground">{userRoleLabel}</p>
                  </div>
                  <UserMenuItem
                    icon={Settings}
                    label="Ajustes de cuenta"
                    onSelect={() => navigateFromUserMenu('/settings')}
                  />
                  {canManageUsers && (
                    <UserMenuItem
                      icon={Shield}
                      label="Usuarios y roles"
                      onSelect={() => navigateFromUserMenu('/users')}
                    />
                  )}
                  <UserMenuItem
                    icon={CreditCard}
                    label="Suscripcion"
                    onSelect={() => navigateFromUserMenu('/subscription')}
                  />
                  <div className="my-1 h-px bg-muted" />
                  <UserMenuItem
                    icon={LogOut}
                    label="Cerrar sesion"
                    onSelect={handleLogout}
                  />
                </div>
              )}
            </div>
          </div>
        </aside>

        {/* ── Topbar ─────────────────────────────────────────────────────── */}
        <header className="sticky top-0 z-20 hidden h-16 shrink-0 items-center justify-between border-b border-border bg-card px-6 lg:flex">
          <div>
            <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
              SaasCommerce
            </p>
            <h1 className="text-lg font-semibold text-foreground">{pageTitle}</h1>
          </div>
          <div className="flex items-center gap-3">
            <NotificationBell />
            <div className="text-right">
              <p className="text-sm font-semibold text-foreground">
                {session?.user.fullName ?? 'Admin'}
              </p>
              <p className="text-xs font-medium text-muted-foreground">{businessName}</p>
            </div>
          </div>
        </header>

        {/* ── Main content ───────────────────────────────────────────────── */}
        <main className="min-h-0 min-w-0 overflow-y-auto overflow-x-hidden bg-background">
          {isHome && !bannerDismissed && (
            <SubscriptionAlertBanner
              className="sticky top-0 z-10 rounded-none border-x-0 border-t-0"
              onChoosePlan={() => navigate('/subscription')}
              onReactivateClick={() => navigate('/subscription')}
              onContactSupport={() => {
                window.location.href = 'mailto:soporte@comercioflowrd.com?subject=Soporte%20suscripcion'
              }}
              onDismiss={dismissBanner}
            />
          )}
          <Outlet />
        </main>
      </div>

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

function getPageTitle(pathname: string) {
  if (pageTitles[pathname]) {
    return pageTitles[pathname]
  }

  if (pathname.startsWith('/cash/') && pathname !== '/cash/history') {
    return 'Detalle de caja'
  }

  if (pathname.startsWith('/daily-closing/') && pathname !== '/daily-closing/history') {
    return 'Detalle de cierre'
  }

  if (pathname.startsWith('/sales/')) {
    return 'Detalle de venta'
  }

  if (pathname.startsWith('/inventory/products/')) {
    return 'Detalle de inventario'
  }

  if (pathname.startsWith('/purchases/')) {
    return pathname === '/purchases/new' ? 'Nueva compra' : 'Detalle de compra'
  }

  if (pathname.startsWith('/invoices/')) {
    return 'Detalle de recibo'
  }

  return 'Inicio'
}

function formatRoleLabel(role?: string) {
  if (!role) return 'Administrador'

  const roleLabels: Record<string, string> = {
    Admin: 'Administrador',
    Cashier: 'Cajero',
    InventoryManager: 'Inventario',
    Owner: 'Propietario',
    PurchasingManager: 'Compras',
    ReadOnly: 'Lectura',
    SaasAdmin: 'Admin plataforma',
    Supervisor: 'Supervisor',
  }

  return roleLabels[role] ?? role
}

type UserMenuItemProps = {
  icon: LucideIcon
  label: string
  onSelect: () => void | Promise<void>
}

function UserMenuItem({ icon: Icon, label, onSelect }: Readonly<UserMenuItemProps>) {
  return (
    <button
      className="flex w-full items-center gap-2.5 px-3 py-2 text-left text-sm font-medium text-foreground transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-ring/20"
      onClick={onSelect}
      role="menuitem"
      type="button"
    >
      <Icon aria-hidden="true" className="shrink-0 text-muted-foreground" size={16} />
      <span className="truncate">{label}</span>
    </button>
  )
}

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

function NavigationSection({
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

  // ── Collapsed + grouped → popup-menu pattern (like the user-menu button) ──
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

  // ── Standard layout: expanded mode accordion OR standalone collapsed items ──
  const itemsVisible = !label || expanded

  return (
    <div className={cn('flex gap-1 lg:flex-col lg:gap-0.5', collapsed && 'lg:items-center')}>
      {/* Expanded sidebar: text-only header (no icon — items already have them) */}
      {hasToggle && !collapsed && (
        <button
          aria-expanded={expanded}
          className={cn(
            'group/header hidden w-full cursor-pointer items-center justify-between gap-2 rounded-md',
            'px-3 py-1.5 text-[10.5px] font-semibold uppercase tracking-[0.1em] text-gray-500',
            'transition-colors hover:bg-sidebar-hover-bg hover:text-gray-900',
            'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-gray-400/30',
            'lg:flex',
          )}
          onClick={onToggle}
          type="button"
        >
          <span>{label}</span>
          <ChevronRight
            aria-hidden="true"
            className={cn(
              'shrink-0 text-gray-400 transition-transform duration-200',
              'group-hover/header:text-gray-700',
              expanded && 'rotate-90',
            )}
            size={12}
            strokeWidth={2.5}
          />
        </button>
      )}

      {itemsVisible && visibleItems.map((item) => (
        <NavLink
          className={({ isActive }) =>
            cn(
              'group relative flex h-9 shrink-0 items-center gap-3 rounded-md px-3 text-[13.5px] font-medium text-sidebar-fg transition-colors',
              'hover:bg-sidebar-hover-bg hover:text-gray-900',
              'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-gray-400/30',
              // Active: white card lift + subtle ring for definition + darker text
              isActive && 'bg-sidebar-active-bg text-sidebar-active-fg font-semibold shadow-sm ring-1 ring-gray-200 hover:bg-sidebar-active-bg hover:text-sidebar-active-fg',
              collapsed && 'lg:h-9 lg:w-9 lg:justify-center lg:px-0',
            )
          }
          end={item.path === '/'}
          key={item.path}
          title={item.label}
          to={item.path}
        >
          {({ isActive }) => (
            <>
              <item.icon
                aria-hidden="true"
                className={cn(
                  'shrink-0 transition-colors',
                  isActive ? 'text-gray-900' : 'text-gray-500 group-hover:text-gray-900',
                )}
                size={17}
                strokeWidth={isActive ? 2.25 : 1.85}
              />
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

// ─── Collapsed group popup (portaled, fixed positioning) ─────────────────────
// Renders OUTSIDE the sidebar via createPortal so it can't be clipped by the
// nav's overflow. Position is computed from the trigger button's bounding box.

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

  // Compute popup position relative to the trigger button. Re-runs on open
  // and on viewport resize (close to avoid stale positions on scroll).
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
          'flex h-9 w-9 cursor-pointer items-center justify-center rounded-md text-[12px] font-semibold uppercase transition-colors',
          popupOpen
            ? 'bg-sidebar-active-bg text-gray-900 shadow-sm ring-1 ring-gray-200'
            : 'text-gray-500 hover:bg-sidebar-hover-bg hover:text-gray-900',
        )}
        onClick={onPopupToggle}
        onMouseEnter={onPopupHoverEnter}
        onMouseLeave={onPopupHoverLeave}
        ref={triggerRef}
        title={label}
        type="button"
      >
        {label.slice(0, 2)}
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
                    'flex items-center gap-2.5 px-3 py-2 text-sm font-medium transition-colors',
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
                <item.icon
                  aria-hidden="true"
                  className="shrink-0 text-gray-500"
                  size={15}
                  strokeWidth={1.85}
                />
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
