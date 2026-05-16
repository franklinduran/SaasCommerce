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

export function ProductsRoute() {
  return <ProductsPage />
}

export function InventoryRoute() {
  return <InventoryPage />
}

export function CustomersRoute() {
  return <CustomersPage />
}

export function PurchasesRoute() {
  return <PurchasesPage />
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
