import {
  ArrowLeftRight,
  Banknote,
  BarChart3,
  Boxes,
  Building2,
  CalendarCheck,
  CircleDollarSign,
  ClipboardList,
  CreditCard,
  GitBranch,
  History,
  LayoutDashboard,
  MessageSquareWarning,
  Package,
  ReceiptText,
  Shield,
  ShoppingCart,
  TrendingUp,
  Truck,
  Users,
  Wallet,
} from 'lucide-react'
import type { LucideIcon } from 'lucide-react'
import { Permission } from '@/shared/types/permissions'
import type { PermissionCode } from '@/shared/types/permissions'

export type NavigationItem = {
  label: string
  path: string
  icon: LucideIcon
  /** Permission(s) required to see this item. Undefined = always visible. */
  requiredPermission?: PermissionCode | PermissionCode[]
}

export type NavigationGroup = {
  /** Optional section heading. Omit for standalone items (e.g. Inicio). */
  label?: string
  items: readonly NavigationItem[]
}

/**
 * Navigation organized into compact, scannable groups.
 * Each group surfaces 2-5 related actions. Order = priority of use during
 * the daily workflow of a colmado / retail business.
 */
export const navigationGroups: readonly NavigationGroup[] = [
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
      { label: 'Caja', path: '/cash-register', icon: Wallet, requiredPermission: Permission.CashView },
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
      { label: 'Feedback Beta', path: '/beta-feedback', icon: MessageSquareWarning, requiredPermission: Permission.BetaFeedbackView },
    ],
  },
]

export const pageTitles: Record<string, string> = {
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
  '/audit-logs': 'Auditoría',
  '/subscription': 'Mi Suscripción',
  '/beta-feedback': 'Feedback Beta',
  '/admin/pilot-metrics': 'Métricas del Piloto',
  '/settings': 'Ajustes',
  '/forbidden': 'Acceso denegado',
}

export const SIDEBAR_EXPANDED_GROUP_KEY = 'sidebar-expanded-group-v2'
export const BANNER_DISMISSED_KEY = 'subscription-banner-dismissed'

export function loadExpandedGroup(): string | null {
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

export function getPageTitle(pathname: string): string {
  if (pageTitles[pathname]) return pageTitles[pathname]

  if (pathname.startsWith('/cash/') && pathname !== '/cash/history') return 'Detalle de caja'
  if (pathname.startsWith('/daily-closing/') && pathname !== '/daily-closing/history') return 'Detalle de cierre'
  if (pathname.startsWith('/sales/')) return 'Detalle de venta'
  if (pathname.startsWith('/inventory/products/')) return 'Detalle de inventario'
  if (pathname.startsWith('/purchases/')) return pathname === '/purchases/new' ? 'Nueva compra' : 'Detalle de compra'
  if (pathname.startsWith('/invoices/')) return 'Detalle de recibo'

  return 'Inicio'
}

export function formatRoleLabel(role?: string): string {
  if (!role) return 'Administrador'

  const roleLabels: Record<string, string> = {
    Admin: 'Administrador',
    Cashier: 'Cajero',
    InventoryManager: 'Inventario',
    Owner: 'Propietario',
    PurchasingManager: 'Compras',
    ReadOnly: 'Solo lectura',
    SaasAdmin: 'Admin plataforma',
    Supervisor: 'Supervisor',
  }

  return roleLabels[role] ?? role
}
