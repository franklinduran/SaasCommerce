namespace SaasCommerce.Modules.Identity.Contracts;

/// <summary>
/// Granular permission codes used to protect API endpoints and UI actions.
/// Permissions are resolved from the user's role via <see cref="RolePermissionMatrix"/>.
/// </summary>
public static class SystemPermissions
{
  // Dashboard
  public const string DashboardView = "dashboard.view";

  // Sales
  public const string SalesView = "sales.view";
  public const string SalesCreate = "sales.create";
  public const string SalesCancel = "sales.cancel";

  // Products / Catalog
  public const string ProductsView = "products.view";
  public const string ProductsCreate = "products.create";
  public const string ProductsUpdate = "products.update";
  public const string ProductsDelete = "products.delete";

  // Inventory
  public const string InventoryView = "inventory.view";
  public const string InventoryAdjust = "inventory.adjust";
  public const string InventoryTransfer = "inventory.transfer";

  // Customers
  public const string CustomersView = "customers.view";
  public const string CustomersCreate = "customers.create";
  public const string CustomersUpdate = "customers.update";

  // Accounts receivable
  public const string AccountsReceivableView = "accountsReceivable.view";
  public const string AccountsReceivableRegisterPayment = "accountsReceivable.registerPayment";

  // Purchases
  public const string PurchasesView = "purchases.view";
  public const string PurchasesCreate = "purchases.create";
  public const string PurchasesReceive = "purchases.receive";
  public const string PurchasesCancel = "purchases.cancel";

  // Invoices
  public const string InvoicesView = "invoices.view";
  public const string InvoicesCreate = "invoices.create";
  public const string InvoicesCancel = "invoices.cancel";

  // Reports
  public const string ReportsView = "reports.view";
  public const string ReportsExport = "reports.export";

  // Users
  public const string UsersView = "users.view";
  public const string UsersCreate = "users.create";
  public const string UsersUpdate = "users.update";
  public const string UsersUpdateRole = "users.updateRole";
  public const string UsersDisable = "users.disable";
  public const string UsersResetPassword = "users.resetPassword";

  // Audit
  public const string AuditView = "audit.view";

  // Branches
  public const string BranchesView = "branches.view";
  public const string BranchesCreate = "branches.create";
  public const string BranchesUpdate = "branches.update";

  // Settings
  public const string SettingsView = "settings.view";
  public const string SettingsUpdate = "settings.update";

  // Cash Register (Caja)
  public const string CashView = "cash.view";
  public const string CashOpen = "cash.open";
  public const string CashClose = "cash.close";
  public const string CashRegisterMovement = "cash.registerMovement";
}
