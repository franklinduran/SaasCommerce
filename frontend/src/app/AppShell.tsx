import {
  ArrowLeftRight,
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
import { useEffect, useState } from 'react'
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
  { label: 'Metricas Piloto', path: '/admin/pilot-metrics', icon: BarChart2, requiredPermission: Permission.SaasPilotMetrics },
  { label: 'Ajustes', path: '/settings', icon: Settings },
]

const mainNavigation = navigationItems.slice(0, 9)
const growthTools = navigationItems.slice(9)

const pageTitles: Record<string, string> = {
  '/': 'Inicio',
  '/pos': 'POS',
  '/cash': 'Caja',
  '/cash/history': 'Historial de cajas',
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

  async function handleLogout() {
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
        <aside
          className={cn(
            'flex w-full min-w-0 max-w-full shrink-0 flex-col border-b border-stone-200 bg-stone-50 text-stone-900 lg:row-span-2 lg:max-h-dvh lg:min-h-0 lg:border-b-0 lg:border-r',
            sidebarCollapsed ? 'lg:w-[76px]' : 'lg:w-[260px]',
          )}
        >
          <div
            className={cn(
              'flex h-16 shrink-0 items-center justify-between gap-2 border-b border-stone-200/60 px-4',
              sidebarCollapsed && 'lg:h-16 lg:px-3',
            )}
          >
            <div className={cn('flex min-w-0 items-center gap-2.5', sidebarCollapsed && 'lg:justify-center')}>
              <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-md bg-stone-900 text-white shadow-sm">
                <CircleDollarSign aria-hidden="true" size={18} />
              </span>
              <div className={cn('min-w-0', sidebarCollapsed && 'lg:hidden')}>
                <p className="truncate text-sm font-semibold text-stone-900">{businessName}</p>
                <p className="truncate text-xs font-medium text-stone-500">
                  {session?.user.fullName ?? 'Sucursal principal'}
                </p>
              </div>
            </div>
            <Button
              className={cn(
                'shrink-0 max-lg:hidden lg:inline-flex',
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

          {!sidebarCollapsed && (
            <div className="hidden px-3 py-3 lg:block">
              <Button
                aria-label="Buscar"
                className="w-full justify-start px-3"
                type="button"
                variant="secondary"
              >
                <Search aria-hidden="true" size={15} />
                <span className="font-medium">Buscar...</span>
                <kbd className="ml-auto rounded bg-stone-100 px-1.5 py-0.5 font-mono text-[10px] font-semibold text-stone-500 ring-1 ring-stone-200">
                  Ctrl K
                </kbd>
              </Button>
            </div>
          )}

          {sidebarCollapsed && (
            <div className="hidden px-2 py-2 lg:block">
              <Button
                aria-label="Expandir menu"
                className="w-full"
                onClick={toggleSidebar}
                size="icon"
                title="Expandir menu"
                variant="ghost"
              >
                <ToggleSidebarIcon aria-hidden="true" size={16} strokeWidth={2} />
              </Button>
            </div>
          )}

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

          <div className={cn('hidden shrink-0 border-t border-stone-200/60 p-3 lg:block', sidebarCollapsed && 'lg:px-2')}>
            {!sidebarCollapsed && (
              <div className="mb-2 flex items-center gap-2.5 rounded-md px-2 py-1.5">
                <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-stone-900 text-xs font-semibold text-white">
                  {session?.user.fullName.slice(0, 1).toUpperCase() ?? 'A'}
                </div>
                <div className="min-w-0 flex-1">
                  <p className="truncate text-sm font-semibold text-stone-900">
                    {session?.user.fullName ?? 'Admin'}
                  </p>
                  <p className="truncate text-xs font-medium text-stone-500">Administrador</p>
                </div>
                <ChevronDown aria-hidden="true" className="text-stone-400" size={14} />
              </div>
            )}
            <Button
              className={cn(
                'w-full justify-start',
                sidebarCollapsed && 'justify-center',
              )}
              onClick={handleLogout}
              variant="ghost"
            >
              <LogOut size={16} />
              {!sidebarCollapsed && <span>Cerrar sesion</span>}
            </Button>
          </div>
        </aside>

        <header className="sticky top-0 z-20 hidden h-16 shrink-0 items-center justify-between border-b border-stone-200 bg-white px-6 lg:flex">
          <div>
            <p className="text-xs font-semibold uppercase tracking-wide text-stone-500">
              SaasCommerce
            </p>
            <h1 className="text-lg font-semibold text-stone-900">{pageTitle}</h1>
          </div>
          <div className="flex items-center gap-3">
            <NotificationBell />
            <div className="text-right">
              <p className="text-sm font-semibold text-stone-900">
                {session?.user.fullName ?? 'Admin'}
              </p>
              <p className="text-xs font-medium text-stone-500">{businessName}</p>
            </div>
          </div>
        </header>

        <main className="min-h-0 min-w-0 overflow-y-auto overflow-x-hidden bg-surface-subtle">
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
        <p className="hidden px-3 pb-1.5 pt-1 text-[10px] font-semibold uppercase tracking-[0.08em] text-stone-400 lg:block">
          {label}
        </p>
      )}
      {visibleItems.map((item) => (
        <NavLink
          className={({ isActive }) =>
            cn(
              'group relative flex h-9 shrink-0 items-center gap-3 rounded-md px-3 text-sm font-medium text-stone-600 transition-colors hover:bg-stone-200/60 hover:text-stone-900',
              'active:bg-stone-300 active:text-stone-950 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-stone-900/25 focus-visible:ring-offset-2',
              isActive && 'bg-stone-900 text-white font-semibold shadow-sm ring-1 ring-stone-900 hover:bg-stone-900 hover:text-white active:bg-stone-950 active:text-white',
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
                  isActive ? 'text-white' : 'text-stone-500 group-hover:text-stone-900',
                )}
                size={17}
                strokeWidth={isActive ? 2.25 : 2}
              />
              <span className={cn('truncate', isActive && 'text-white', collapsed && 'lg:hidden')}>
                {item.label}
              </span>
            </>
          )}
        </NavLink>
      ))}
    </div>
  )
}
