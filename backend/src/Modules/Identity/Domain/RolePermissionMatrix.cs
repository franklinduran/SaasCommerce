using SaasCommerce.Modules.Identity.Contracts;

namespace SaasCommerce.Modules.Identity.Domain;

/// <summary>
/// Static matrix that maps each system role to its set of granted permissions.
/// This is the authoritative source for the MVP permission model.
/// </summary>
public static class RolePermissionMatrix
{
  private static readonly IReadOnlySet<string> FullAccess = new HashSet<string>(StringComparer.Ordinal)
  {
    SystemPermissions.DashboardView,

    SystemPermissions.SalesView,
    SystemPermissions.SalesCreate,
    SystemPermissions.SalesCancel,

    SystemPermissions.ProductsView,
    SystemPermissions.ProductsCreate,
    SystemPermissions.ProductsUpdate,
    SystemPermissions.ProductsDelete,

    SystemPermissions.InventoryView,
    SystemPermissions.InventoryAdjust,
    SystemPermissions.InventoryTransfer,

    SystemPermissions.CustomersView,
    SystemPermissions.CustomersCreate,
    SystemPermissions.CustomersUpdate,

    SystemPermissions.AccountsReceivableView,
    SystemPermissions.AccountsReceivableRegisterPayment,

    SystemPermissions.PurchasesView,
    SystemPermissions.PurchasesCreate,
    SystemPermissions.PurchasesReceive,
    SystemPermissions.PurchasesCancel,

    SystemPermissions.InvoicesView,
    SystemPermissions.InvoicesCreate,
    SystemPermissions.InvoicesCancel,

    SystemPermissions.ReportsView,
    SystemPermissions.ReportsExport,

    SystemPermissions.UsersView,
    SystemPermissions.UsersCreate,
    SystemPermissions.UsersUpdate,
    SystemPermissions.UsersUpdateRole,
    SystemPermissions.UsersDisable,
    SystemPermissions.UsersResetPassword,

    SystemPermissions.AuditView,

    SystemPermissions.BranchesView,
    SystemPermissions.BranchesCreate,
    SystemPermissions.BranchesUpdate,

    SystemPermissions.SettingsView,
    SystemPermissions.SettingsUpdate,

    SystemPermissions.CashView,
    SystemPermissions.CashOpen,
    SystemPermissions.CashClose,
    SystemPermissions.CashRegisterMovement,

    SystemPermissions.ExpensesView,
    SystemPermissions.ExpensesCreate,
    SystemPermissions.ExpensesManage,
    SystemPermissions.ExpenseCategoriesManage,

    SystemPermissions.ProfitabilityView,
    SystemPermissions.ProfitabilityProducts,
    SystemPermissions.ProfitabilityBranches,
    SystemPermissions.ProfitabilityAlerts,

    SystemPermissions.DailyClosingView,
    SystemPermissions.DailyClosingPreview,
    SystemPermissions.DailyClosingCreate,
    SystemPermissions.DailyClosingClose,
    SystemPermissions.DailyClosingHistory,

    SystemPermissions.NotificationsView,
    SystemPermissions.NotificationsRead,
    SystemPermissions.NotificationsManage,

    SystemPermissions.SaasManageBusinesses,
    SystemPermissions.SaasPilotMetrics,

    SystemPermissions.OnboardingView,
    SystemPermissions.OnboardingManage,

    SystemPermissions.ProductsImport,

    SystemPermissions.ProductsExport,
    SystemPermissions.SalesExport,
    SystemPermissions.InventoryExport,
    SystemPermissions.CustomersExport,
    SystemPermissions.CreditsExport,
    SystemPermissions.CashExport,
    SystemPermissions.CashRegisterDailySummary
  };

  private static readonly IReadOnlySet<string> SupervisorAccess = new HashSet<string>(StringComparer.Ordinal)
  {
    SystemPermissions.DashboardView,

    SystemPermissions.SalesView,
    SystemPermissions.SalesCreate,
    SystemPermissions.SalesCancel,

    SystemPermissions.ProductsView,
    SystemPermissions.ProductsUpdate,

    SystemPermissions.InventoryView,
    SystemPermissions.InventoryAdjust,

    SystemPermissions.CustomersView,
    SystemPermissions.CustomersCreate,
    SystemPermissions.CustomersUpdate,

    SystemPermissions.AccountsReceivableView,
    SystemPermissions.AccountsReceivableRegisterPayment,

    SystemPermissions.PurchasesView,
    SystemPermissions.PurchasesReceive,

    SystemPermissions.InvoicesView,
    SystemPermissions.InvoicesCreate,
    SystemPermissions.InvoicesCancel,

    SystemPermissions.ReportsView,
    SystemPermissions.ReportsExport,

    SystemPermissions.UsersView,

    SystemPermissions.BranchesView,

    SystemPermissions.CashView,
    SystemPermissions.CashOpen,
    SystemPermissions.CashClose,
    SystemPermissions.CashRegisterMovement,

    SystemPermissions.ExpensesView,
    SystemPermissions.ExpensesCreate,
    SystemPermissions.ExpensesManage,
    SystemPermissions.ExpenseCategoriesManage,

    SystemPermissions.ProfitabilityView,
    SystemPermissions.ProfitabilityProducts,
    SystemPermissions.ProfitabilityBranches,
    SystemPermissions.ProfitabilityAlerts,

    SystemPermissions.DailyClosingView,
    SystemPermissions.DailyClosingPreview,
    SystemPermissions.DailyClosingCreate,
    SystemPermissions.DailyClosingClose,
    SystemPermissions.DailyClosingHistory,

    SystemPermissions.NotificationsView,
    SystemPermissions.NotificationsRead,

    SystemPermissions.OnboardingView,

    SystemPermissions.ProductsImport,
    SystemPermissions.CashRegisterDailySummary
  };

  private static readonly IReadOnlySet<string> CashierAccess = new HashSet<string>(StringComparer.Ordinal)
  {
    SystemPermissions.DashboardView,

    SystemPermissions.SalesView,
    SystemPermissions.SalesCreate,

    SystemPermissions.ProductsView,

    SystemPermissions.CustomersView,
    SystemPermissions.CustomersCreate,

    SystemPermissions.AccountsReceivableView,
    SystemPermissions.AccountsReceivableRegisterPayment,

    SystemPermissions.InvoicesView,

    SystemPermissions.CashView,
    SystemPermissions.CashOpen,
    SystemPermissions.CashClose,
    SystemPermissions.CashRegisterMovement,

    SystemPermissions.ExpensesView,
    SystemPermissions.ExpensesCreate,

    SystemPermissions.DailyClosingView,
    SystemPermissions.DailyClosingPreview,
    SystemPermissions.DailyClosingCreate,
    SystemPermissions.DailyClosingClose,

    SystemPermissions.NotificationsView,
    SystemPermissions.NotificationsRead
  };

  private static readonly IReadOnlySet<string> InventoryManagerAccess = new HashSet<string>(StringComparer.Ordinal)
  {
    SystemPermissions.DashboardView,

    SystemPermissions.ProductsView,
    SystemPermissions.ProductsCreate,
    SystemPermissions.ProductsUpdate,

    SystemPermissions.InventoryView,
    SystemPermissions.InventoryAdjust,
    SystemPermissions.InventoryTransfer,

    SystemPermissions.BranchesView,

    SystemPermissions.PurchasesView,

    SystemPermissions.ReportsView
  };

  private static readonly IReadOnlySet<string> PurchasingManagerAccess = new HashSet<string>(StringComparer.Ordinal)
  {
    SystemPermissions.DashboardView,

    SystemPermissions.ProductsView,

    SystemPermissions.InventoryView,

    SystemPermissions.PurchasesView,
    SystemPermissions.PurchasesCreate,
    SystemPermissions.PurchasesReceive,
    SystemPermissions.PurchasesCancel,

    SystemPermissions.ReportsView
  };

  private static readonly IReadOnlySet<string> ReadOnlyAccess = new HashSet<string>(StringComparer.Ordinal)
  {
    SystemPermissions.DashboardView,

    SystemPermissions.SalesView,

    SystemPermissions.ProductsView,

    SystemPermissions.InventoryView,

    SystemPermissions.CustomersView,

    SystemPermissions.AccountsReceivableView,

    SystemPermissions.PurchasesView,

    SystemPermissions.InvoicesView,

    SystemPermissions.ReportsView
  };

  /// <summary>
  /// Returns the set of permissions for a given role name, or an empty set if the role is unknown.
  /// </summary>
  public static IReadOnlySet<string> GetPermissions(string roleName)
  {
    return roleName switch
    {
      SystemRoles.Owner or SystemRoles.Admin => FullAccess,
      SystemRoles.Supervisor => SupervisorAccess,
      SystemRoles.Cashier => CashierAccess,
      SystemRoles.InventoryManager => InventoryManagerAccess,
      SystemRoles.PurchasingManager => PurchasingManagerAccess,
      SystemRoles.ReadOnly => ReadOnlyAccess,
      _ => new HashSet<string>(StringComparer.Ordinal)
    };
  }

  /// <summary>
  /// Returns the union of permissions for a set of role names.
  /// </summary>
  public static IReadOnlySet<string> GetPermissions(IEnumerable<string> roleNames)
  {
    var permissions = new HashSet<string>(StringComparer.Ordinal);

    foreach (var role in roleNames)
    {
      foreach (var permission in GetPermissions(role))
      {
        permissions.Add(permission);
      }
    }

    return permissions;
  }
}
