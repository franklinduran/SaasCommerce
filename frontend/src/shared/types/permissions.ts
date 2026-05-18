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
} as const

export type PermissionCode = (typeof Permission)[keyof typeof Permission]

export type CurrentUserPermissionsResponse = {
  userId: string
  businessId: string
  role: string
  permissions: string[]
}
