import { Suspense } from 'react'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import {
  AuditLogsRoute,
  AuthRoute,
  BranchesRoute,
  BranchProfitabilityRoute,
  CashHistoryRoute,
  CashRoute,
  CashSessionDetailRoute,
  CustomersRoute,
  DailyClosingDetailRoute,
  DailyClosingHistoryRoute,
  DailyClosingRoute,
  DashboardRoute,
  ExpenseCategoriesRoute,
  ExpenseDetailRoute,
  ExpensesRoute,
  ForbiddenRoute,
  InventoryProductDetailRoute,
  InventoryRoute,
  InventoryTransfersRoute,
  InvoicesRoute,
  NewExpenseRoute,
  NewPurchaseRoute,
  NotificationsRoute,
  OnboardingRoute,
  PilotBusinessRoute,
  PilotMetricsRoute,
  POSRoute,
  ProductImportRoute,
  ProductProfitabilityRoute,
  ProductsRoute,
  ProfitabilityAlertsRoute,
  ProfitabilityRoute,
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

function mockedPage(label: string) {
  return function MockedPage() {
    return <div>{label}</div>
  }
}

vi.mock('@/modules/auth/AuthPage', () => ({ AuthPage: mockedPage('AuthPage') }))
vi.mock('@/modules/onboarding/pages/OnboardingPage', () => ({ OnboardingPage: mockedPage('OnboardingPage') }))
vi.mock('@/modules/admin/pages/PilotBusinessPage', () => ({ PilotBusinessPage: mockedPage('PilotBusinessPage') }))
vi.mock('@/modules/admin/pages/PilotMetricsPage', () => ({ PilotMetricsPage: mockedPage('PilotMetricsPage') }))
vi.mock('@/modules/products/pages/ProductImportPage', () => ({ ProductImportPage: mockedPage('ProductImportPage') }))
vi.mock('@/modules/account/RegisterBusinessPage', () => ({ RegisterBusinessPage: mockedPage('RegisterBusinessPage') }))
vi.mock('@/modules/dashboard/DashboardPage', () => ({ DashboardPage: mockedPage('DashboardPage') }))
vi.mock('@/modules/sales/SalesPage', () => ({ SalesPage: mockedPage('SalesPage') }))
vi.mock('@/modules/sales/pages/SaleDetailPage', () => ({ SaleDetailPage: mockedPage('SaleDetailPage') }))
vi.mock('@/modules/pos/pages/POSPage', () => ({ POSPage: mockedPage('POSPage') }))
vi.mock('@/modules/products/ProductsPage', () => ({ ProductsPage: mockedPage('ProductsPage') }))
vi.mock('@/modules/inventory/InventoryPage', () => ({ InventoryPage: mockedPage('InventoryPage') }))
vi.mock('@/modules/inventory/pages/InventoryProductDetailPage', () => ({ InventoryProductDetailPage: mockedPage('InventoryProductDetailPage') }))
vi.mock('@/modules/customers/CustomersPage', () => ({ CustomersPage: mockedPage('CustomersPage') }))
vi.mock('@/modules/purchases/PurchasesPage', () => ({ PurchasesPage: mockedPage('PurchasesPage') }))
vi.mock('@/modules/purchases/pages/NewPurchasePage', () => ({ NewPurchasePage: mockedPage('NewPurchasePage') }))
vi.mock('@/modules/purchases/pages/PurchaseDetailPage', () => ({ PurchaseDetailPage: mockedPage('PurchaseDetailPage') }))
vi.mock('@/modules/suppliers/SuppliersPage', () => ({ SuppliersPage: mockedPage('SuppliersPage') }))
vi.mock('@/modules/invoices/InvoicesPage', () => ({ InvoicesPage: mockedPage('InvoicesPage') }))
vi.mock('@/modules/reports/ReportsPage', () => ({ ReportsPage: mockedPage('ReportsPage') }))
vi.mock('@/modules/settings/SettingsPage', () => ({ SettingsPage: mockedPage('SettingsPage') }))
vi.mock('@/modules/users/UsersPage', () => ({ default: mockedPage('UsersPage') }))
vi.mock('@/modules/audit/AuditLogsPage', () => ({ default: mockedPage('AuditLogsPage') }))
vi.mock('@/shared/components/ForbiddenPage', () => ({ ForbiddenPage: mockedPage('ForbiddenPage') }))
vi.mock('@/modules/branches/pages/BranchesPage', () => ({ BranchesPage: mockedPage('BranchesPage') }))
vi.mock('@/modules/inventory-transfers/pages/InventoryTransfersPage', () => ({ InventoryTransfersPage: mockedPage('InventoryTransfersPage') }))
vi.mock('@/modules/subscription/pages/SubscriptionPage', () => ({ SubscriptionPage: mockedPage('SubscriptionPage') }))
vi.mock('@/modules/cash/pages/CashPage', () => ({ CashPage: mockedPage('CashPage') }))
vi.mock('@/modules/cash/pages/CashHistoryPage', () => ({ CashHistoryPage: mockedPage('CashHistoryPage') }))
vi.mock('@/modules/cash/pages/CashSessionDetailPage', () => ({ CashSessionDetailPage: mockedPage('CashSessionDetailPage') }))
vi.mock('@/modules/expenses/pages/ExpensesPage', () => ({ ExpensesPage: mockedPage('ExpensesPage') }))
vi.mock('@/modules/expenses/pages/NewExpensePage', () => ({ NewExpensePage: mockedPage('NewExpensePage') }))
vi.mock('@/modules/expenses/pages/ExpenseDetailPage', () => ({ ExpenseDetailPage: mockedPage('ExpenseDetailPage') }))
vi.mock('@/modules/expenses/pages/ExpenseCategoriesPage', () => ({ ExpenseCategoriesPage: mockedPage('ExpenseCategoriesPage') }))
vi.mock('@/modules/profitability/pages/ProfitabilityPage', () => ({ ProfitabilityPage: mockedPage('ProfitabilityPage') }))
vi.mock('@/modules/profitability/pages/ProductProfitabilityPage', () => ({ ProductProfitabilityPage: mockedPage('ProductProfitabilityPage') }))
vi.mock('@/modules/profitability/pages/BranchProfitabilityPage', () => ({ BranchProfitabilityPage: mockedPage('BranchProfitabilityPage') }))
vi.mock('@/modules/profitability/pages/ProfitabilityAlertsPage', () => ({ ProfitabilityAlertsPage: mockedPage('ProfitabilityAlertsPage') }))
vi.mock('@/modules/daily-closing/pages/DailyClosingPage', () => ({ DailyClosingPage: mockedPage('DailyClosingPage') }))
vi.mock('@/modules/daily-closing/pages/DailyClosingHistoryPage', () => ({ DailyClosingHistoryPage: mockedPage('DailyClosingHistoryPage') }))
vi.mock('@/modules/daily-closing/pages/DailyClosingDetailPage', () => ({ DailyClosingDetailPage: mockedPage('DailyClosingDetailPage') }))
vi.mock('@/modules/notifications/pages/NotificationListPage', () => ({ NotificationListPage: mockedPage('NotificationListPage') }))

describe('LazyPages routes', () => {
  afterEach(() => cleanup())

  const cases: Array<[string, React.ComponentType]> = [
    ['AuthPage', AuthRoute],
    ['RegisterBusinessPage', RegisterBusinessRoute],
    ['DashboardPage', DashboardRoute],
    ['SalesPage', SalesRoute],
    ['SaleDetailPage', SaleDetailRoute],
    ['POSPage', POSRoute],
    ['ProductsPage', ProductsRoute],
    ['InventoryPage', InventoryRoute],
    ['InventoryProductDetailPage', InventoryProductDetailRoute],
    ['CustomersPage', CustomersRoute],
    ['PurchasesPage', PurchasesRoute],
    ['NewPurchasePage', NewPurchaseRoute],
    ['PurchaseDetailPage', PurchaseDetailRoute],
    ['SuppliersPage', SuppliersRoute],
    ['InvoicesPage', InvoicesRoute],
    ['ReportsPage', ReportsRoute],
    ['SettingsPage', SettingsRoute],
    ['UsersPage', UsersRoute],
    ['AuditLogsPage', AuditLogsRoute],
    ['ForbiddenPage', ForbiddenRoute],
    ['BranchesPage', BranchesRoute],
    ['InventoryTransfersPage', InventoryTransfersRoute],
    ['SubscriptionPage', SubscriptionRoute],
    ['CashPage', CashRoute],
    ['CashHistoryPage', CashHistoryRoute],
    ['CashSessionDetailPage', CashSessionDetailRoute],
    ['ExpensesPage', ExpensesRoute],
    ['NewExpensePage', NewExpenseRoute],
    ['ExpenseDetailPage', ExpenseDetailRoute],
    ['ExpenseCategoriesPage', ExpenseCategoriesRoute],
    ['ProfitabilityPage', ProfitabilityRoute],
    ['ProductProfitabilityPage', ProductProfitabilityRoute],
    ['BranchProfitabilityPage', BranchProfitabilityRoute],
    ['ProfitabilityAlertsPage', ProfitabilityAlertsRoute],
    ['DailyClosingPage', DailyClosingRoute],
    ['DailyClosingHistoryPage', DailyClosingHistoryRoute],
    ['DailyClosingDetailPage', DailyClosingDetailRoute],
    ['NotificationListPage', NotificationsRoute],
    ['OnboardingPage', OnboardingRoute],
    ['PilotBusinessPage', PilotBusinessRoute],
    ['PilotMetricsPage', PilotMetricsRoute],
    ['ProductImportPage', ProductImportRoute],
  ]

  it.each(cases)('renders %s through its lazy route', async (label, Route) => {
    render(
      <Suspense fallback={<span>loading</span>}>
        <Route />
      </Suspense>,
    )

    expect(await screen.findByText(label)).toBeTruthy()
  })
})
