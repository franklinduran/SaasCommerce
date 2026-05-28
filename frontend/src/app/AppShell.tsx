import {
  ArrowLeftRight,
  Banknote,
  BarChart2,
  BarChart3,
  Boxes,
  Building2,
  CalendarCheck,
  ChevronDown,
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
import { useEffect, useRef, useState } from 'react'
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

const navigationItems: readonly NavigationItem[] = [
  { label: 'Inicio', path: '/', icon: LayoutDashboard, requiredPermission: Permission.DashboardView },
  { label: 'POS', path: '/pos', icon: ShoppingCart, requiredPermission: Permission.SalesCreate },
  { label: 'Caja', path: '/cash', icon: Wallet, requiredPermission: Permission.CashView },
  { label: 'Arqueo Caja', path: '/cash-register', icon: Banknote, requiredPermission: Permission.CashView },
  { label: 'Cierre Diario', path: '/daily-closing', icon: CalendarCheck, requiredPermission: Permission.DailyClosingView },
  { label: 'Gastos', path: '/expenses', icon: CircleDollarSign, requiredPermission: Permission.ExpensesView },
  { label: 'Ventas', path: '/sales', icon: History, requiredPermission: Permission.SalesView },
  { label: 'Productos', path: '/products', icon: Package, requiredPermission: Permission.ProductsView },
  { label: 'Inventario', path: '/inventory', icon: Boxes, requiredPermission: Permission.InventoryView },
  { label: 'Transferencias', path: '/inventory-transfers', icon: ArrowLeftRight, requiredPermission: Permission.InventoryTransfer },
  { label: 'Sucursales', path: '/branches', icon: GitBranch, requiredPermission: Permission.BranchesView },
  { label: 'Clientes', path: '/customers', icon: Users, requiredPermission: Permission.CustomersView },
  { label: 'Proveedores', path: '/suppliers', icon: Building2, requiredPermission: Permission.PurchasesView },
  { label: 'Compras', path: '/purchases', icon: Truck, requiredPermission: Permission.PurchasesView },
  { label: 'Recibos', path: '/invoices', icon: ReceiptText, requiredPermission: Permission.InvoicesView },
  { label: 'Reportes', path: '/reports', icon: BarChart3, requiredPermission: Permission.ReportsView },
  { label: 'Rentabilidad', path: '/profitability', icon: TrendingUp, requiredPermission: Permission.ProfitabilityView },
  { label: 'Usuarios', path: '/users', icon: Shield, requiredPermission: Permission.UsersView },
  { label: 'Auditoria', path: '/audit-logs', icon: ClipboardList, requiredPermission: Permission.AuditView },
  { label: 'Suscripcion', path: '/subscription', icon: CreditCard },
  { label: 'Feedback Beta', path: '/beta-feedback', icon: MessageSquareWarning, requiredPermission: Permission.BetaFeedbackView },
  { label: 'Metricas Piloto', path: '/admin/pilot-metrics', icon: BarChart2, requiredPermission: Permission.SaasPilotMetrics },
  { label: 'Ajustes', path: '/settings', icon: Settings },
]

const mainNavigation = navigationItems.slice(0, 10)
const growthTools = navigationItems.slice(10)

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
  const ToggleSidebarIcon = sidebarCollapsed ? PanelLeftOpen : PanelLeftClose
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
            'flex w-full min-w-0 max-w-full shrink-0 flex-col border-b border-sidebar-border bg-sidebar-bg text-sidebar-fg lg:row-span-2 lg:max-h-dvh lg:min-h-0 lg:border-b-0 lg:border-r',
            sidebarCollapsed ? 'lg:w-[76px]' : 'lg:w-[260px]',
          )}
        >
          {/* Logo / business header */}
          <div
            className={cn(
              'flex h-16 shrink-0 items-center justify-between gap-2 border-b border-sidebar-border/60 px-4',
              sidebarCollapsed && 'lg:h-16 lg:px-3',
            )}
          >
            <div className={cn('flex min-w-0 items-center gap-2.5', sidebarCollapsed && 'lg:justify-center')}>
              <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-md bg-sidebar-active-bg text-sidebar-active-fg shadow-sm">
                <CircleDollarSign aria-hidden="true" size={18} />
              </span>
              <div className={cn('min-w-0', sidebarCollapsed && 'lg:hidden')}>
                <p className="truncate text-sm font-semibold text-sidebar-fg">{businessName}</p>
                <p className="truncate text-xs font-medium text-sidebar-muted">
                  {session?.user.fullName ?? 'Sucursal principal'}
                </p>
              </div>
            </div>
            <Button
              className={cn(
                'shrink-0 max-lg:hidden lg:inline-flex text-sidebar-fg hover:bg-sidebar-hover-bg hover:text-sidebar-fg focus-visible:ring-sidebar-active-bg/25',
                sidebarCollapsed && 'lg:hidden',
              )}
              aria-label="Contraer menu"
              onClick={toggleSidebar}
              size="icon"
              title="Contraer menu"
              variant="ghost"
            >
              <ToggleSidebarIcon aria-hidden="true" size={16} strokeWidth={2} />
            </Button>
          </div>

          {/* Search bar (expanded) */}
          {!sidebarCollapsed && (
            <div className="hidden px-3 py-3 lg:block">
              <Button
                aria-label="Buscar"
                className="w-full justify-start px-3 bg-sidebar-hover-bg text-sidebar-fg ring-1 ring-sidebar-border/60 hover:bg-sidebar-hover-bg/80 hover:text-sidebar-fg focus-visible:ring-sidebar-active-bg/25"
                type="button"
                variant="ghost"
              >
                <Search aria-hidden="true" size={15} />
                <span className="font-medium">Buscar...</span>
                <kbd className="ml-auto rounded bg-sidebar-border/40 px-1.5 py-0.5 font-mono text-[10px] font-semibold text-sidebar-muted ring-1 ring-sidebar-border/60">
                  Ctrl K
                </kbd>
              </Button>
            </div>
          )}

          {/* Expand button (collapsed) */}
          {sidebarCollapsed && (
            <div className="hidden px-2 py-2 lg:block">
              <Button
                aria-label="Expandir menu"
                className="w-full text-sidebar-fg hover:bg-sidebar-hover-bg hover:text-sidebar-fg focus-visible:ring-sidebar-active-bg/25"
                onClick={toggleSidebar}
                size="icon"
                title="Expandir menu"
                variant="ghost"
              >
                <ToggleSidebarIcon aria-hidden="true" size={16} strokeWidth={2} />
              </Button>
            </div>
          )}

          {/* Navigation */}
          <nav
            className={cn(
              'flex min-w-0 shrink-0 gap-1 overflow-x-auto px-3 py-2 lg:min-h-0 lg:flex-1 lg:flex-col lg:gap-3 lg:overflow-y-auto lg:overflow-x-hidden lg:py-2',
              sidebarCollapsed && 'lg:px-2',
            )}
          >
            <NavigationSection
              collapsed={sidebarCollapsed}
              items={mainNavigation}
              label="Navegacion"
              userPermissions={userPermissions}
            />
            <NavigationSection
              collapsed={sidebarCollapsed}
              items={growthTools}
              label="Operacion"
              userPermissions={userPermissions}
            />
          </nav>

          {/* User menu trigger */}
          <div className={cn('hidden shrink-0 border-t border-sidebar-border/60 p-3 lg:block', sidebarCollapsed && 'lg:px-2')}>
            <div className="relative" ref={userMenuRef}>
              <button
                aria-expanded={userMenuOpen}
                aria-haspopup="menu"
                aria-label="Abrir menu de usuario"
                className={cn(
                  'flex w-full items-center gap-2.5 rounded-md px-2 py-1.5 text-left transition-colors hover:bg-sidebar-hover-bg focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-sidebar-active-bg/25 focus-visible:ring-offset-2 focus-visible:ring-offset-sidebar-bg',
                  sidebarCollapsed && 'h-10 justify-center px-0',
                )}
                onClick={() => setUserMenuOpen((open) => !open)}
                type="button"
              >
                <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-sidebar-active-bg text-xs font-semibold text-sidebar-active-fg">
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
                  className="absolute bottom-full left-0 z-30 mb-2 w-64 overflow-hidden rounded-xl border border-border bg-card py-1 shadow-lg ring-1 ring-black/5"
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
  items: readonly NavigationItem[]
  label: string
  userPermissions: string[]
}

function NavigationSection({ collapsed, items, label, userPermissions }: Readonly<NavigationSectionProps>) {
  const visibleItems = items.filter((item) => {
    if (!item.requiredPermission) return true
    const required = Array.isArray(item.requiredPermission)
      ? item.requiredPermission
      : [item.requiredPermission]
    return required.some((p) => userPermissions.includes(p))
  })

  if (visibleItems.length === 0) return null

  return (
    <div className="flex gap-1 lg:flex-col lg:gap-0.5">
      {!collapsed && (
        <p className="hidden px-3 pb-1.5 pt-1 text-[10px] font-semibold uppercase tracking-[0.08em] text-sidebar-muted lg:block">
          {label}
        </p>
      )}
      {visibleItems.map((item) => (
        <NavLink
          className={({ isActive }) =>
            cn(
              'group relative flex h-9 shrink-0 items-center gap-3 rounded-md px-3 text-sm font-medium text-sidebar-fg transition-colors hover:bg-sidebar-hover-bg hover:text-sidebar-fg',
              'active:bg-sidebar-hover-bg/80 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-sidebar-active-bg/25 focus-visible:ring-offset-2 focus-visible:ring-offset-sidebar-bg',
              isActive && 'bg-sidebar-active-bg text-sidebar-active-fg font-semibold shadow-sm ring-1 ring-sidebar-active-bg/50 hover:bg-sidebar-active-bg hover:text-sidebar-active-fg',
              collapsed && 'lg:h-10 lg:w-10 lg:justify-center lg:px-0',
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
                  isActive ? 'text-sidebar-active-fg' : 'text-sidebar-muted group-hover:text-sidebar-fg',
                )}
                size={17}
                strokeWidth={isActive ? 2.25 : 2}
              />
              <span className={cn('truncate', isActive && 'text-sidebar-active-fg', collapsed && 'lg:hidden')}>
                {item.label}
              </span>
            </>
          )}
        </NavLink>
      ))}
    </div>
  )
}
