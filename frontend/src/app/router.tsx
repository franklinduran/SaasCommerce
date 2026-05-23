import { Suspense } from 'react'
import type { ReactNode } from 'react'
import { createBrowserRouter } from 'react-router-dom'
import { AppShell } from '@/app/AppShell'
import {
  AuditLogsRoute,
  AuthRoute,
  BranchesRoute,
  BranchProfitabilityRoute,
  CashHistoryRoute,
  CashRoute,
  CashSessionDetailRoute,
  ExpenseCategoriesRoute,
  ExpenseDetailRoute,
  ExpensesRoute,
  NewExpenseRoute,
  CustomersRoute,
  DashboardRoute,
  ForbiddenRoute,
  InventoryProductDetailRoute,
  InventoryRoute,
  InventoryTransfersRoute,
  InvoicesRoute,
  NewPurchaseRoute,
  ProductsRoute,
  POSRoute,
  ProfitabilityAlertsRoute,
  ProfitabilityRoute,
  ProductProfitabilityRoute,
  PurchaseDetailRoute,
  PurchasesRoute,
  RegisterBusinessRoute,
  ReportsRoute,
  SaleDetailRoute,
  SalesRoute,
  SettingsRoute,
  SubscriptionRoute,
  SuppliersRoute,
  UsersRoute,
} from '@/app/LazyPages'
import { ProtectedRoute } from '@/modules/auth/components/ProtectedRoute'
import { SubscriptionGate } from '@/modules/subscription/components/SubscriptionGate'
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
        <SubscriptionGate>
          <AppShell />
        </SubscriptionGate>
      </ProtectedRoute>
    ),
    children: [
      { index: true, element: withPageLoading(<DashboardRoute />) },
      { path: 'pos', element: withPageLoading(<POSRoute />) },
      { path: 'cash', element: withPageLoading(<CashRoute />) },
      { path: 'cash/history', element: withPageLoading(<CashHistoryRoute />) },
      { path: 'cash/:cashSessionId', element: withPageLoading(<CashSessionDetailRoute />) },
      { path: 'expenses', element: withPageLoading(<ExpensesRoute />) },
      { path: 'expenses/new', element: withPageLoading(<NewExpenseRoute />) },
      { path: 'expenses/categories', element: withPageLoading(<ExpenseCategoriesRoute />) },
      { path: 'expenses/:id', element: withPageLoading(<ExpenseDetailRoute />) },
      { path: 'sales', element: withPageLoading(<SalesRoute />) },
      { path: 'sales/:saleId', element: withPageLoading(<SaleDetailRoute />) },
      { path: 'products', element: withPageLoading(<ProductsRoute />) },
      { path: 'inventory', element: withPageLoading(<InventoryRoute />) },
      { path: 'inventory/products/:productId', element: withPageLoading(<InventoryProductDetailRoute />) },
      { path: 'inventory-transfers', element: withPageLoading(<InventoryTransfersRoute />) },
      { path: 'branches', element: withPageLoading(<BranchesRoute />) },
      { path: 'customers', element: withPageLoading(<CustomersRoute />) },
      { path: 'customers/:customerId', element: withPageLoading(<CustomersRoute />) },
      { path: 'suppliers', element: withPageLoading(<SuppliersRoute />) },
      { path: 'purchases', element: withPageLoading(<PurchasesRoute />) },
      { path: 'purchases/new', element: withPageLoading(<NewPurchaseRoute />) },
      { path: 'purchases/:purchaseId', element: withPageLoading(<PurchaseDetailRoute />) },
      { path: 'invoices', element: withPageLoading(<InvoicesRoute />) },
      { path: 'invoices/:invoiceId', element: withPageLoading(<InvoicesRoute />) },
      { path: 'reports', element: withPageLoading(<ReportsRoute />) },
      { path: 'profitability', element: withPageLoading(<ProfitabilityRoute />) },
      { path: 'profitability/products', element: withPageLoading(<ProductProfitabilityRoute />) },
      { path: 'profitability/branches', element: withPageLoading(<BranchProfitabilityRoute />) },
      { path: 'profitability/alerts', element: withPageLoading(<ProfitabilityAlertsRoute />) },
      { path: 'users', element: withPageLoading(<UsersRoute />) },
      { path: 'users/new', element: withPageLoading(<UsersRoute />) },
      { path: 'users/:userId', element: withPageLoading(<UsersRoute />) },
      { path: 'audit-logs', element: withPageLoading(<AuditLogsRoute />) },
      { path: 'forbidden', element: withPageLoading(<ForbiddenRoute />) },
      { path: 'settings', element: withPageLoading(<SettingsRoute />) },
      { path: 'subscription', element: withPageLoading(<SubscriptionRoute />) },
    ],
  },
])
