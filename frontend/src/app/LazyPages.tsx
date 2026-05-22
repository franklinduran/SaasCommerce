import { lazy } from 'react'

const AuthPage = lazy(() =>
  import('@/modules/auth/AuthPage').then((module) => ({ default: module.AuthPage })),
)
const RegisterBusinessPage = lazy(() =>
  import('@/modules/account/RegisterBusinessPage').then((module) => ({
    default: module.RegisterBusinessPage,
  })),
)
const DashboardPage = lazy(() =>
  import('@/modules/dashboard/DashboardPage').then((module) => ({
    default: module.DashboardPage,
  })),
)
const SalesPage = lazy(() =>
  import('@/modules/sales/SalesPage').then((module) => ({ default: module.SalesPage })),
)
const SaleDetailPage = lazy(() =>
  import('@/modules/sales/pages/SaleDetailPage').then((module) => ({
    default: module.SaleDetailPage,
  })),
)
const POSPage = lazy(() =>
  import('@/modules/pos/pages/POSPage').then((module) => ({ default: module.POSPage })),
)
const ProductsPage = lazy(() =>
  import('@/modules/products/ProductsPage').then((module) => ({
    default: module.ProductsPage,
  })),
)
const InventoryPage = lazy(() =>
  import('@/modules/inventory/InventoryPage').then((module) => ({
    default: module.InventoryPage,
  })),
)
const InventoryProductDetailPage = lazy(() =>
  import('@/modules/inventory/pages/InventoryProductDetailPage').then((module) => ({
    default: module.InventoryProductDetailPage,
  })),
)
const CustomersPage = lazy(() =>
  import('@/modules/customers/CustomersPage').then((module) => ({
    default: module.CustomersPage,
  })),
)
const PurchasesPage = lazy(() =>
  import('@/modules/purchases/PurchasesPage').then((module) => ({
    default: module.PurchasesPage,
  })),
)
const NewPurchasePage = lazy(() =>
  import('@/modules/purchases/pages/NewPurchasePage').then((module) => ({
    default: module.NewPurchasePage,
  })),
)
const PurchaseDetailPage = lazy(() =>
  import('@/modules/purchases/pages/PurchaseDetailPage').then((module) => ({
    default: module.PurchaseDetailPage,
  })),
)
const SuppliersPage = lazy(() =>
  import('@/modules/suppliers/SuppliersPage').then((module) => ({
    default: module.SuppliersPage,
  })),
)
const InvoicesPage = lazy(() =>
  import('@/modules/invoices/InvoicesPage').then((module) => ({
    default: module.InvoicesPage,
  })),
)
const ReportsPage = lazy(() =>
  import('@/modules/reports/ReportsPage').then((module) => ({
    default: module.ReportsPage,
  })),
)
const SettingsPage = lazy(() =>
  import('@/modules/settings/SettingsPage').then((module) => ({
    default: module.SettingsPage,
  })),
)
const UsersPage = lazy(() => import('@/modules/users/UsersPage'))
const AuditLogsPage = lazy(() => import('@/modules/audit/AuditLogsPage'))
const ForbiddenPage = lazy(() =>
  import('@/shared/components/ForbiddenPage').then((module) => ({
    default: module.ForbiddenPage,
  })),
)
const BranchesPage = lazy(() =>
  import('@/modules/branches/pages/BranchesPage').then((module) => ({
    default: module.BranchesPage,
  })),
)
const InventoryTransfersPage = lazy(() =>
  import('@/modules/inventory-transfers/pages/InventoryTransfersPage').then((module) => ({
    default: module.InventoryTransfersPage,
  })),
)

export function AuthRoute() {
  return <AuthPage />
}

export function RegisterBusinessRoute() {
  return <RegisterBusinessPage />
}

export function DashboardRoute() {
  return <DashboardPage />
}

export function SalesRoute() {
  return <SalesPage />
}

export function SaleDetailRoute() {
  return <SaleDetailPage />
}

export function POSRoute() {
  return <POSPage />
}

export function ProductsRoute() {
  return <ProductsPage />
}

export function InventoryRoute() {
  return <InventoryPage />
}

export function InventoryProductDetailRoute() {
  return <InventoryProductDetailPage />
}

export function CustomersRoute() {
  return <CustomersPage />
}

export function PurchasesRoute() {
  return <PurchasesPage />
}

export function NewPurchaseRoute() {
  return <NewPurchasePage />
}

export function PurchaseDetailRoute() {
  return <PurchaseDetailPage />
}

export function SuppliersRoute() {
  return <SuppliersPage />
}

export function InvoicesRoute() {
  return <InvoicesPage />
}

export function ReportsRoute() {
  return <ReportsPage />
}

export function SettingsRoute() {
  return <SettingsPage />
}

export function UsersRoute() {
  return <UsersPage />
}

export function AuditLogsRoute() {
  return <AuditLogsPage />
}

export function ForbiddenRoute() {
  return <ForbiddenPage />
}

export function BranchesRoute() {
  return <BranchesPage />
}

export function InventoryTransfersRoute() {
  return <InventoryTransfersPage />
}
