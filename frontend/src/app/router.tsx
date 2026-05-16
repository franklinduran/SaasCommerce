import { Suspense } from 'react'
import type { ReactNode } from 'react'
import { createBrowserRouter } from 'react-router-dom'
import { AppShell } from '@/app/AppShell'
import {
  AuthRoute,
  CustomersRoute,
  DashboardRoute,
  InventoryRoute,
  InvoicesRoute,
  ProductsRoute,
  PurchasesRoute,
  RegisterBusinessRoute,
  ReportsRoute,
  SalesRoute,
  SettingsRoute,
} from '@/app/LazyPages'
import { ProtectedRoute } from '@/modules/auth/components/ProtectedRoute'
import { PageLoadingState } from '@/shared/components/PageLoadingState'

function withPageLoading(element: ReactNode) {
  return <Suspense fallback={<PageLoadingState />}>{element}</Suspense>
}

export const router = createBrowserRouter([
  { path: 'auth', element: withPageLoading(<AuthRoute />) },
  { path: 'register-business', element: withPageLoading(<RegisterBusinessRoute />) },
  {
    element: (
      <ProtectedRoute>
        <AppShell />
      </ProtectedRoute>
    ),
    children: [
      { index: true, element: withPageLoading(<DashboardRoute />) },
      { path: 'sales', element: withPageLoading(<SalesRoute />) },
      { path: 'products', element: withPageLoading(<ProductsRoute />) },
      { path: 'inventory', element: withPageLoading(<InventoryRoute />) },
      { path: 'customers', element: withPageLoading(<CustomersRoute />) },
      { path: 'purchases', element: withPageLoading(<PurchasesRoute />) },
      { path: 'invoices', element: withPageLoading(<InvoicesRoute />) },
      { path: 'reports', element: withPageLoading(<ReportsRoute />) },
      { path: 'settings', element: withPageLoading(<SettingsRoute />) },
    ],
  },
])
