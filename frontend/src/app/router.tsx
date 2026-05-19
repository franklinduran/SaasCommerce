import { Suspense } from 'react'
import type { ReactNode } from 'react'
import { createBrowserRouter } from 'react-router-dom'
import { AppShell } from '@/app/AppShell'
import {
  AuditLogsRoute,
  AuthRoute,
  CustomerDetailRoute,
  CustomersRoute,
  DashboardRoute,
  ForbiddenRoute,
  InventoryProductDetailRoute,
  InvoiceDetailRoute,
  InventoryRoute,
  InvoicesRoute,
  NewPurchaseRoute,
  ProductsRoute,
  POSRoute,
  PurchaseDetailRoute,
  PurchasesRoute,
  RegisterBusinessRoute,
  ReportsRoute,
  SaleDetailRoute,
  SalesRoute,
  SettingsRoute,
  SuppliersRoute,
  UsersRoute,
} from '@/app/LazyPages'
import { ProtectedRoute } from '@/modules/auth/components/ProtectedRoute'
import { PageLoadingState } from '@/shared/components/PageLoadingState'

function withPageLoading(element: ReactNode) {
  return <Suspense fallback={<PageLoadingState />}>{element}</Suspense>
}

export const router = createBrowserRouter([
  { path: 'auth', element: withPageLoading(<AuthRoute />) },
  { path: 'login', element: withPageLoading(<AuthRoute />) },
  { path: 'register-business', element: withPageLoading(<RegisterBusinessRoute />) },
  {
    element: (
      <ProtectedRoute>
        <AppShell />
      </ProtectedRoute>
    ),
    children: [
      { index: true, element: withPageLoading(<DashboardRoute />) },
      { path: 'pos', element: withPageLoading(<POSRoute />) },
      { path: 'sales', element: withPageLoading(<SalesRoute />) },
      { path: 'sales/:saleId', element: withPageLoading(<SaleDetailRoute />) },
      { path: 'products', element: withPageLoading(<ProductsRoute />) },
      { path: 'inventory', element: withPageLoading(<InventoryRoute />) },
      { path: 'inventory/products/:productId', element: withPageLoading(<InventoryProductDetailRoute />) },
      { path: 'customers', element: withPageLoading(<CustomersRoute />) },
      { path: 'customers/:customerId', element: withPageLoading(<CustomerDetailRoute />) },
      { path: 'suppliers', element: withPageLoading(<SuppliersRoute />) },
      { path: 'purchases', element: withPageLoading(<PurchasesRoute />) },
      { path: 'purchases/new', element: withPageLoading(<NewPurchaseRoute />) },
      { path: 'purchases/:purchaseId', element: withPageLoading(<PurchaseDetailRoute />) },
      { path: 'invoices', element: withPageLoading(<InvoicesRoute />) },
      { path: 'invoices/:invoiceId', element: withPageLoading(<InvoiceDetailRoute />) },
      { path: 'reports', element: withPageLoading(<ReportsRoute />) },
      { path: 'users', element: withPageLoading(<UsersRoute />) },
      { path: 'users/new', element: withPageLoading(<UsersRoute />) },
      { path: 'users/:userId', element: withPageLoading(<UsersRoute />) },
      { path: 'audit-logs', element: withPageLoading(<AuditLogsRoute />) },
      { path: 'forbidden', element: withPageLoading(<ForbiddenRoute />) },
      { path: 'settings', element: withPageLoading(<SettingsRoute />) },
    ],
  },
])
