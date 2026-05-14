import {
  BarChart3,
  Boxes,
  LayoutDashboard,
  LogOut,
  Menu,
  Package,
  ReceiptText,
  ShoppingCart,
  Truck,
  Users,
} from 'lucide-react'
import type { LucideIcon } from 'lucide-react'
import { NavLink, Outlet } from 'react-router-dom'
import { Button } from '@/shared/components/ui/button'
import { useAppStore } from '@/shared/hooks/useAppStore'
import { cn } from '@/shared/utils/cn'

type NavigationItem = {
  label: string
  path: string
  icon: LucideIcon
}

const navigationItems: NavigationItem[] = [
  { label: 'Dashboard', path: '/', icon: LayoutDashboard },
  { label: 'POS', path: '/sales', icon: ShoppingCart },
  { label: 'Productos', path: '/products', icon: Package },
  { label: 'Inventario', path: '/inventory', icon: Boxes },
  { label: 'Clientes', path: '/customers', icon: Users },
  { label: 'Compras', path: '/purchases', icon: Truck },
  { label: 'Facturas', path: '/invoices', icon: ReceiptText },
  { label: 'Reportes', path: '/reports', icon: BarChart3 },
]

export function AppShell() {
  const businessName = useAppStore((state) => state.businessName)
  const sidebarCollapsed = useAppStore((state) => state.sidebarCollapsed)
  const toggleSidebar = useAppStore((state) => state.toggleSidebar)

  return (
    <div className="min-h-screen bg-background text-foreground lg:grid lg:grid-cols-[auto_1fr]">
      <aside
        className={cn(
          'border-b border-border bg-white lg:min-h-screen lg:border-b-0 lg:border-r',
          sidebarCollapsed ? 'lg:w-[88px]' : 'lg:w-[260px]',
        )}
      >
        <div className="flex h-16 items-center justify-between px-4">
          <div className="min-w-0">
            <p className="truncate text-sm font-semibold text-primary">{businessName}</p>
            {!sidebarCollapsed && (
              <p className="text-xs text-slate-500">Sucursal principal</p>
            )}
          </div>
          <Button
            aria-label="Alternar menu"
            onClick={toggleSidebar}
            size="icon"
            title="Alternar menu"
            variant="ghost"
          >
            <Menu size={18} />
          </Button>
        </div>

        <nav className="flex gap-1 overflow-x-auto px-3 pb-3 lg:flex-col lg:overflow-visible">
          {navigationItems.map((item) => (
            <NavLink
              className={({ isActive }) =>
                cn(
                  'flex h-10 shrink-0 items-center gap-3 rounded-md px-3 text-sm font-medium text-slate-700 transition-colors hover:bg-muted',
                  isActive && 'bg-muted text-primary',
                  sidebarCollapsed && 'lg:justify-center lg:px-0',
                )
              }
              end={item.path === '/'}
              key={item.path}
              title={item.label}
              to={item.path}
            >
              <item.icon aria-hidden="true" size={18} />
              {!sidebarCollapsed && <span>{item.label}</span>}
            </NavLink>
          ))}
        </nav>

        <div className="hidden px-3 pt-3 lg:block">
          <Button className="w-full" variant="outline">
            <LogOut size={16} />
            {!sidebarCollapsed && <span>Salir</span>}
          </Button>
        </div>
      </aside>

      <Outlet />
    </div>
  )
}
