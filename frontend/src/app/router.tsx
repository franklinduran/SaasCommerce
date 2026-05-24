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
  DailyClosingDetailRoute,
  DailyClosingHistoryRoute,
  DailyClosingRoute,
  NotificationsRoute,
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
import { PermissionRoute } from '@/modules/auth/components/PermissionRoute'
import { ProtectedRoute } from '@/modules/auth/components/ProtectedRoute'
import { SubscriptionGate } from '@/modules/subscription/components/SubscriptionGate'
import { PageLoadingState } from '@/shared/components/PageLoadingState'
import { Permission } from '@/shared/types/permissions'

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
      {
        path: 'pos',
        element: (
          <PermissionRoute permissions={Permission.SalesCreate}>
            {withPageLoading(<POSRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'cash',
        element: (
          <PermissionRoute permissions={Permission.CashView}>
            {withPageLoading(<CashRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'cash/history',
        element: (
          <PermissionRoute permissions={Permission.CashView}>
            {withPageLoading(<CashHistoryRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'cash/:cashSessionId',
        element: (
          <PermissionRoute permissions={Permission.CashView}>
            {withPageLoading(<CashSessionDetailRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'expenses',
        element: (
          <PermissionRoute permissions={Permission.ExpensesView}>
            {withPageLoading(<ExpensesRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'expenses/new',
        element: (
          <PermissionRoute permissions={Permission.ExpensesCreate}>
            {withPageLoading(<NewExpenseRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'expenses/categories',
        element: (
          <PermissionRoute permissions={Permission.ExpenseCategoriesManage}>
            {withPageLoading(<ExpenseCategoriesRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'expenses/:id',
        element: (
          <PermissionRoute permissions={Permission.ExpensesView}>
            {withPageLoading(<ExpenseDetailRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'sales',
        element: (
          <PermissionRoute permissions={Permission.SalesView}>
            {withPageLoading(<SalesRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'sales/:saleId',
        element: (
          <PermissionRoute permissions={Permission.SalesView}>
            {withPageLoading(<SaleDetailRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'products',
        element: (
          <PermissionRoute permissions={Permission.ProductsView}>
            {withPageLoading(<ProductsRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'inventory',
        element: (
          <PermissionRoute permissions={Permission.InventoryView}>
            {withPageLoading(<InventoryRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'inventory/products/:productId',
        element: (
          <PermissionRoute permissions={Permission.InventoryView}>
            {withPageLoading(<InventoryProductDetailRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'inventory-transfers',
        element: (
          <PermissionRoute permissions={Permission.InventoryTransfer}>
            {withPageLoading(<InventoryTransfersRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'branches',
        element: (
          <PermissionRoute permissions={Permission.BranchesView}>
            {withPageLoading(<BranchesRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'customers',
        element: (
          <PermissionRoute permissions={Permission.CustomersView}>
            {withPageLoading(<CustomersRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'customers/:customerId',
        element: (
          <PermissionRoute permissions={Permission.CustomersView}>
            {withPageLoading(<CustomersRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'suppliers',
        element: (
          <PermissionRoute permissions={Permission.PurchasesView}>
            {withPageLoading(<SuppliersRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'purchases',
        element: (
          <PermissionRoute permissions={Permission.PurchasesView}>
            {withPageLoading(<PurchasesRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'purchases/new',
        element: (
          <PermissionRoute permissions={Permission.PurchasesCreate}>
            {withPageLoading(<NewPurchaseRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'purchases/:purchaseId',
        element: (
          <PermissionRoute permissions={Permission.PurchasesView}>
            {withPageLoading(<PurchaseDetailRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'invoices',
        element: (
          <PermissionRoute permissions={Permission.InvoicesView}>
            {withPageLoading(<InvoicesRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'invoices/:invoiceId',
        element: (
          <PermissionRoute permissions={Permission.InvoicesView}>
            {withPageLoading(<InvoicesRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'reports',
        element: (
          <PermissionRoute permissions={Permission.ReportsView}>
            {withPageLoading(<ReportsRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'profitability',
        element: (
          <PermissionRoute permissions={Permission.ProfitabilityView}>
            {withPageLoading(<ProfitabilityRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'profitability/products',
        element: (
          <PermissionRoute permissions={Permission.ProfitabilityProducts}>
            {withPageLoading(<ProductProfitabilityRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'profitability/branches',
        element: (
          <PermissionRoute permissions={Permission.ProfitabilityBranches}>
            {withPageLoading(<BranchProfitabilityRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'profitability/alerts',
        element: (
          <PermissionRoute permissions={Permission.ProfitabilityAlerts}>
            {withPageLoading(<ProfitabilityAlertsRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'users',
        element: (
          <PermissionRoute permissions={Permission.UsersView}>
            {withPageLoading(<UsersRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'users/new',
        element: (
          <PermissionRoute permissions={Permission.UsersInvite}>
            {withPageLoading(<UsersRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'users/:userId',
        element: (
          <PermissionRoute permissions={Permission.UsersView}>
            {withPageLoading(<UsersRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'audit-logs',
        element: (
          <PermissionRoute permissions={Permission.AuditView}>
            {withPageLoading(<AuditLogsRoute />)}
          </PermissionRoute>
        ),
      },
      { path: 'forbidden', element: withPageLoading(<ForbiddenRoute />) },
      { path: 'settings', element: withPageLoading(<SettingsRoute />) },
      { path: 'subscription', element: withPageLoading(<SubscriptionRoute />) },
      {
        path: 'daily-closing',
        element: (
          <PermissionRoute permissions={Permission.DailyClosingView}>
            {withPageLoading(<DailyClosingRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'daily-closing/history',
        element: (
          <PermissionRoute permissions={Permission.DailyClosingHistory}>
            {withPageLoading(<DailyClosingHistoryRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'daily-closing/:closingId',
        element: (
          <PermissionRoute permissions={Permission.DailyClosingView}>
            {withPageLoading(<DailyClosingDetailRoute />)}
          </PermissionRoute>
        ),
      },
      {
        path: 'notifications',
        element: (
          <PermissionRoute permissions={Permission.NotificationsView}>
            {withPageLoading(<NotificationsRoute />)}
          </PermissionRoute>
        ),
      },
    ],
  },
])
