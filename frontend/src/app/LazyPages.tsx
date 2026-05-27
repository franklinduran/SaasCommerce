import { lazy } from 'react'

const AuthPage = lazy(() =>
  import('@/modules/auth/AuthPage').then((module) => ({ default: module.AuthPage })),
)
const OnboardingPage = lazy(() =>
  import('@/modules/onboarding/pages/OnboardingPage').then((module) => ({
    default: module.OnboardingPage,
  })),
)
const PilotBusinessPage = lazy(() =>
  import('@/modules/admin/pages/PilotBusinessPage').then((module) => ({
    default: module.PilotBusinessPage,
  })),
)
const PilotMetricsPage = lazy(() =>
  import('@/modules/admin/pages/PilotMetricsPage').then((module) => ({
    default: module.PilotMetricsPage,
  })),
)
const BetaFeedbackPage = lazy(() =>
  import('@/modules/beta-feedback/pages/BetaFeedbackPage').then((module) => ({
    default: module.BetaFeedbackPage,
  })),
)
const ProductImportPage = lazy(() =>
  import('@/modules/products/pages/ProductImportPage').then((module) => ({
    default: module.ProductImportPage,
  })),
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
const SubscriptionPage = lazy(() =>
  import('@/modules/subscription/pages/SubscriptionPage').then((module) => ({
    default: module.SubscriptionPage,
  })),
)
const CashPage = lazy(() =>
  import('@/modules/cash/pages/CashPage').then((module) => ({ default: module.CashPage })),
)
const CashHistoryPage = lazy(() =>
  import('@/modules/cash/pages/CashHistoryPage').then((module) => ({
    default: module.CashHistoryPage,
  })),
)
const CashSessionDetailPage = lazy(() =>
  import('@/modules/cash/pages/CashSessionDetailPage').then((module) => ({
    default: module.CashSessionDetailPage,
  })),
)
const ExpensesPage = lazy(() =>
  import('@/modules/expenses/pages/ExpensesPage').then((module) => ({
    default: module.ExpensesPage,
  })),
)
const NewExpensePage = lazy(() =>
  import('@/modules/expenses/pages/NewExpensePage').then((module) => ({
    default: module.NewExpensePage,
  })),
)
const ExpenseDetailPage = lazy(() =>
  import('@/modules/expenses/pages/ExpenseDetailPage').then((module) => ({
    default: module.ExpenseDetailPage,
  })),
)
const ExpenseCategoriesPage = lazy(() =>
  import('@/modules/expenses/pages/ExpenseCategoriesPage').then((module) => ({
    default: module.ExpenseCategoriesPage,
  })),
)
const ProfitabilityPage = lazy(() =>
  import('@/modules/profitability/pages/ProfitabilityPage').then((module) => ({
    default: module.ProfitabilityPage,
  })),
)
const ProductProfitabilityPage = lazy(() =>
  import('@/modules/profitability/pages/ProductProfitabilityPage').then((module) => ({
    default: module.ProductProfitabilityPage,
  })),
)
const BranchProfitabilityPage = lazy(() =>
  import('@/modules/profitability/pages/BranchProfitabilityPage').then((module) => ({
    default: module.BranchProfitabilityPage,
  })),
)
const ProfitabilityAlertsPage = lazy(() =>
  import('@/modules/profitability/pages/ProfitabilityAlertsPage').then((module) => ({
    default: module.ProfitabilityAlertsPage,
  })),
)
const DailyClosingPage = lazy(() =>
  import('@/modules/daily-closing/pages/DailyClosingPage').then((module) => ({
    default: module.DailyClosingPage,
  })),
)
const DailyClosingHistoryPage = lazy(() =>
  import('@/modules/daily-closing/pages/DailyClosingHistoryPage').then((module) => ({
    default: module.DailyClosingHistoryPage,
  })),
)
const DailyClosingDetailPage = lazy(() =>
  import('@/modules/daily-closing/pages/DailyClosingDetailPage').then((module) => ({
    default: module.DailyClosingDetailPage,
  })),
)
const NotificationListPage = lazy(() =>
  import('@/modules/notifications/pages/NotificationListPage').then((module) => ({
    default: module.NotificationListPage,
  })),
)
const CashRegisterPage = lazy(() =>
  import('@/modules/cash-register/pages/CashRegisterPage').then((module) => ({
    default: module.CashRegisterPage,
  })),
)
const CashRegisterHistoryPage = lazy(() =>
  import('@/modules/cash-register/pages/CashRegisterHistoryPage').then((module) => ({
    default: module.CashRegisterHistoryPage,
  })),
)
const DailyCashRegisterSummaryPage = lazy(() =>
  import('@/modules/cash-register/pages/DailyCashRegisterSummaryPage').then((module) => ({
    default: module.DailyCashRegisterSummaryPage,
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

export function SubscriptionRoute() {
  return <SubscriptionPage />
}

export function CashRoute() {
  return <CashPage />
}

export function CashHistoryRoute() {
  return <CashHistoryPage />
}

export function CashSessionDetailRoute() {
  return <CashSessionDetailPage />
}

export function ExpensesRoute() {
  return <ExpensesPage />
}

export function NewExpenseRoute() {
  return <NewExpensePage />
}

export function ExpenseDetailRoute() {
  return <ExpenseDetailPage />
}

export function ExpenseCategoriesRoute() {
  return <ExpenseCategoriesPage />
}

export function ProfitabilityRoute() {
  return <ProfitabilityPage />
}

export function ProductProfitabilityRoute() {
  return <ProductProfitabilityPage />
}

export function BranchProfitabilityRoute() {
  return <BranchProfitabilityPage />
}

export function ProfitabilityAlertsRoute() {
  return <ProfitabilityAlertsPage />
}

export function DailyClosingRoute() {
  return <DailyClosingPage />
}

export function DailyClosingHistoryRoute() {
  return <DailyClosingHistoryPage />
}

export function DailyClosingDetailRoute() {
  return <DailyClosingDetailPage />
}

export function NotificationsRoute() {
  return <NotificationListPage />
}

export function CashRegisterRoute() {
  return <CashRegisterPage />
}

export function CashRegisterHistoryRoute() {
  return <CashRegisterHistoryPage />
}

export function DailyCashRegisterSummaryRoute() {
  return <DailyCashRegisterSummaryPage />
}

export function OnboardingRoute() {
  return <OnboardingPage />
}

export function PilotBusinessRoute() {
  return <PilotBusinessPage />
}

export function PilotMetricsRoute() {
  return <PilotMetricsPage />
}

export function BetaFeedbackRoute() {
  return <BetaFeedbackPage />
}

export function ProductImportRoute() {
  return <ProductImportPage />
}
