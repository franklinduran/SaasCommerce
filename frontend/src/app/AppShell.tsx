import {
  BarChart3,
  Boxes,
  ChevronDown,
  CircleDollarSign,
  History,
  LayoutDashboard,
  LogOut,
  Package,
  PanelLeftClose,
  PanelLeftOpen,
  ReceiptText,
  Search,
  Settings,
  ShoppingCart,
  Truck,
  Users,
} from 'lucide-react'
import type { LucideIcon } from 'lucide-react'
import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useAuthStore } from '@/modules/auth/authStore'
import { Button } from '@/shared/components/ui/button'
import { useAppStore } from '@/shared/hooks/useAppStore'
import { cn } from '@/shared/utils/cn'

type NavigationItem = {
  label: string
  path: string
  icon: LucideIcon
}

const navigationItems: readonly NavigationItem[] = [
  { label: 'Inicio', path: '/', icon: LayoutDashboard },
  { label: 'POS', path: '/pos', icon: ShoppingCart },
  { label: 'Ventas', path: '/sales', icon: History },
  { label: 'Productos', path: '/products', icon: Package },
  { label: 'Inventario', path: '/inventory', icon: Boxes },
  { label: 'Clientes', path: '/customers', icon: Users },
  { label: 'Compras', path: '/purchases', icon: Truck },
  { label: 'Facturas', path: '/invoices', icon: ReceiptText },
  { label: 'Reportes', path: '/reports', icon: BarChart3 },
  { label: 'Ajustes', path: '/settings', icon: Settings },
]

const mainNavigation = navigationItems.slice(0, 5)
const growthTools = navigationItems.slice(5)

const pageTitles: Record<string, string> = {
  '/': 'Inicio',
  '/pos': 'POS',
  '/sales': 'Ventas',
  '/products': 'Productos',
  '/inventory': 'Inventario',
  '/customers': 'Clientes',
  '/purchases': 'Compras',
  '/invoices': 'Facturas',
  '/reports': 'Reportes',
  '/settings': 'Ajustes',
}

export function AppShell() {
  const businessName = useAppStore((state) => state.businessName)
  const sidebarCollapsed = useAppStore((state) => state.sidebarCollapsed)
  const toggleSidebar = useAppStore((state) => state.toggleSidebar)
  const session = useAuthStore((state) => state.session)
  const clearSession = useAuthStore((state) => state.clearSession)
  const location = useLocation()
  const navigate = useNavigate()
  const pageTitle = getPageTitle(location.pathname)
  const ToggleSidebarIcon = sidebarCollapsed ? PanelLeftOpen : PanelLeftClose

  function handleLogout() {
    clearSession()
    navigate('/login', { replace: true })
  }

  return (
    <div className="h-dvh overflow-hidden bg-background text-foreground">
      <div className="grid h-full w-full overflow-hidden bg-background lg:grid-cols-[auto_minmax(0,1fr)] lg:grid-rows-[64px_minmax(0,1fr)]">
        <aside
          className={cn(
            'flex max-h-dvh min-h-0 bg-stone-100 text-stone-900 shadow-[1px_0_0_rgb(231_229_228)] lg:row-span-2 lg:flex-col',
            sidebarCollapsed ? 'lg:w-[88px]' : 'lg:w-[260px]',
          )}
        >
          <div
            className={cn(
              'flex shrink-0 items-center px-4',
              sidebarCollapsed
                ? 'h-[104px] flex-col justify-center gap-3 lg:px-0'
                : 'h-16 justify-between',
            )}
          >
            <div className="flex min-w-0 items-center gap-3">
              <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-md bg-stone-900 text-white">
                <CircleDollarSign aria-hidden="true" size={19} />
              </span>
              {!sidebarCollapsed && (
                <div className="min-w-0">
                  <p className="truncate text-sm font-semibold text-stone-900">{businessName}</p>
                  <p className="truncate text-xs font-medium text-stone-600">
                    {session?.user.fullName ?? 'Sucursal principal'}
                  </p>
                </div>
              )}
            </div>
            <Button
              className={cn(
                'h-9 w-9 shrink-0 rounded-md p-0 text-stone-700 hover:bg-stone-200 hover:text-stone-950',
                sidebarCollapsed && 'bg-white shadow-sm ring-1 ring-stone-200',
              )}
              aria-label={sidebarCollapsed ? 'Expandir menu' : 'Contraer menu'}
              onClick={toggleSidebar}
              size="icon"
              title={sidebarCollapsed ? 'Expandir menu' : 'Contraer menu'}
              variant="ghost"
            >
              <ToggleSidebarIcon aria-hidden="true" size={17} strokeWidth={2} />
            </Button>
          </div>

          {!sidebarCollapsed && (
            <div className="hidden px-4 py-3 lg:block">
              <div className="flex h-10 items-center gap-2 rounded-md bg-white px-3 text-sm text-stone-600 shadow-sm ring-1 ring-stone-200">
                <Search aria-hidden="true" size={16} />
                <span>Buscar...</span>
                <span className="ml-auto rounded bg-stone-100 px-1.5 py-0.5 text-xs text-stone-600">
                  Ctrl K
                </span>
              </div>
            </div>
          )}

          <nav className="flex min-w-0 flex-1 gap-1 overflow-x-auto px-3 py-3 lg:min-h-0 lg:flex-col lg:overflow-y-auto lg:overflow-x-hidden">
            <NavigationSection
              collapsed={sidebarCollapsed}
              items={mainNavigation}
              label="NAVEGACION"
            />
            <NavigationSection
              collapsed={sidebarCollapsed}
              items={growthTools}
              label="OPERACION"
            />
          </nav>

          <div className="hidden shrink-0 p-3 lg:block">
            {!sidebarCollapsed && (
              <div className="mb-3 flex items-center gap-3 rounded-md bg-white p-2 shadow-sm ring-1 ring-stone-200">
                <div className="flex h-9 w-9 items-center justify-center rounded-md bg-stone-900 text-white shadow-control">
                  {session?.user.fullName.slice(0, 1).toUpperCase() ?? 'A'}
                </div>
                <div className="min-w-0 flex-1">
                  <p className="truncate text-sm font-semibold text-stone-900">
                    {session?.user.fullName ?? 'Admin'}
                  </p>
                  <p className="truncate text-xs font-medium text-stone-600">Administrador</p>
                </div>
                <ChevronDown aria-hidden="true" className="text-stone-500" size={16} />
              </div>
            )}
            <Button
              className="w-full bg-white text-stone-900 shadow-sm ring-1 ring-stone-200 hover:bg-stone-50"
              onClick={handleLogout}
              variant="ghost"
            >
              <LogOut size={16} />
              {!sidebarCollapsed && <span>Salir</span>}
            </Button>
          </div>
        </aside>

        <header className="sticky top-0 z-20 hidden h-16 shrink-0 items-center justify-between bg-white px-6 shadow-sm ring-1 ring-stone-200 lg:flex">
          <div>
            <p className="text-xs font-semibold uppercase tracking-wide text-stone-500">
              SaasCommerce
            </p>
            <h1 className="text-lg font-semibold text-stone-900">{pageTitle}</h1>
          </div>
          <div className="text-right">
            <p className="text-sm font-semibold text-stone-900">
              {session?.user.fullName ?? 'Admin'}
            </p>
            <p className="text-xs font-medium text-stone-500">{businessName}</p>
          </div>
        </header>

        <main className="min-h-0 min-w-0 overflow-y-auto overflow-x-hidden">
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

  if (pathname.startsWith('/sales/')) {
    return 'Detalle de venta'
  }

  if (pathname.startsWith('/inventory/products/')) {
    return 'Detalle de inventario'
  }

  return 'Inicio'
}

type NavigationSectionProps = {
  collapsed: boolean
  items: readonly NavigationItem[]
  label: string
}

function NavigationSection({ collapsed, items, label }: Readonly<NavigationSectionProps>) {
  return (
    <div className="flex gap-1 lg:flex-col lg:gap-1.5">
      {!collapsed && (
        <p className="hidden px-3 pb-1 pt-2 text-[11px] font-semibold uppercase text-stone-600 lg:block">
          {label}
        </p>
      )}
      {items.map((item) => (
        <NavLink
          className={({ isActive }) =>
            cn(
              'flex h-10 shrink-0 items-center gap-3 rounded-md px-3 text-sm font-medium text-stone-700 transition-colors hover:bg-stone-200 hover:text-stone-950',
              isActive && 'bg-stone-900 text-white shadow-sm hover:bg-stone-900 hover:text-white',
              collapsed && 'lg:justify-center lg:px-0',
            )
          }
          end={item.path === '/'}
          key={item.path}
          title={item.label}
          to={item.path}
        >
          <item.icon aria-hidden="true" size={18} />
          {!collapsed && <span>{item.label}</span>}
        </NavLink>
      ))}
    </div>
  )
}
