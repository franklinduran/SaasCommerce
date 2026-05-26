/** Permission codes matching backend SystemPermissions. */
export const Permission = {
  // Dashboard
  DashboardView: 'dashboard.view',

  // Sales
  SalesView: 'sales.view',
  SalesCreate: 'sales.create',
  SalesCancel: 'sales.cancel',

  // Products / Catalog
  ProductsView: 'products.view',
  ProductsCreate: 'products.create',
  ProductsUpdate: 'products.update',
  ProductsDelete: 'products.delete',

  // Inventory
  InventoryView: 'inventory.view',
  InventoryAdjust: 'inventory.adjust',
  InventoryTransfer: 'inventory.transfer',

  // Customers
  CustomersView: 'customers.view',
  CustomersCreate: 'customers.create',
  CustomersUpdate: 'customers.update',

  // Accounts receivable
  AccountsReceivableView: 'accountsReceivable.view',
  AccountsReceivableRegisterPayment: 'accountsReceivable.registerPayment',

  // Purchases
  PurchasesView: 'purchases.view',
  PurchasesCreate: 'purchases.create',
  PurchasesReceive: 'purchases.receive',
  PurchasesCancel: 'purchases.cancel',

  // Invoices
  InvoicesView: 'invoices.view',
  InvoicesCreate: 'invoices.create',
  InvoicesCancel: 'invoices.cancel',

  // Reports
  ReportsView: 'reports.view',
  ReportsExport: 'reports.export',

  // Users
  UsersView: 'users.view',
  UsersInvite: 'users.invite',
  UsersUpdateRole: 'users.updateRole',
  UsersDisable: 'users.disable',

  // Audit
  AuditView: 'audit.view',

  // Branches
  BranchesView: 'branches.view',
  BranchesCreate: 'branches.create',
  BranchesUpdate: 'branches.update',

  // Cash Register (Caja)
  CashView: 'cash.view',
  CashOpen: 'cash.open',
  CashClose: 'cash.close',
  CashRegisterMovement: 'cash.registerMovement',
  CashRegisterDailySummary: 'cashRegister.dailySummary',

  // Operating Expenses (Gastos Operativos)
  ExpensesView: 'expenses.view',
  ExpensesCreate: 'expenses.create',
  ExpensesManage: 'expenses.manage',
  ExpenseCategoriesManage: 'expenses.categories.manage',

  // Profitability (Rentabilidad)
  ProfitabilityView: 'profitability.view',
  ProfitabilityProducts: 'profitability.products',
  ProfitabilityBranches: 'profitability.branches',
  ProfitabilityAlerts: 'profitability.alerts',

  // Daily Closing (Cierre Diario)
  DailyClosingView: 'daily_closing.view',
  DailyClosingPreview: 'daily_closing.preview',
  DailyClosingCreate: 'daily_closing.create',
  DailyClosingClose: 'daily_closing.close',
  DailyClosingHistory: 'daily_closing.history',

  // Notifications (Notificaciones)
  NotificationsView: 'notifications.view',
  NotificationsRead: 'notifications.read',
  NotificationsManage: 'notifications.manage',

  // SaaS Platform Administration
  SaasManageBusinesses: 'saas.manage_businesses',

  // Onboarding
  OnboardingView: 'onboarding.view',
  OnboardingManage: 'onboarding.manage',

  // Product Import
  ProductsImport: 'products.import',

  // Exports (CSV / data exports)
  ProductsExport: 'products.export',
  SalesExport: 'sales.export',
  InventoryExport: 'inventory.export',
  CustomersExport: 'customers.export',
  CreditsExport: 'credits.export',
  CashExport: 'cash.export',

  // SaaS Pilot Metrics
  SaasPilotMetrics: 'saas.pilot_metrics',
} as const

export type PermissionCode = (typeof Permission)[keyof typeof Permission]

export type CurrentUserPermissionsResponse = {
  userId: string
  businessId: string
  role: string
  permissions: string[]
}
