namespace SaasCommerce.Modules.Billing.Domain;

/// <summary>
/// Feature flags that can be restricted by subscription plan.
/// Used to enforce limits on what businesses can do based on their current plan.
/// </summary>
[Flags]
public enum SubscriptionFeatures
{
  /// <summary>No features enabled</summary>
  None = 0,

  /// <summary>Sales/transactions functionality</summary>
  Sales = 1 << 0,

  /// <summary>Product management</summary>
  Products = 1 << 1,

  /// <summary>Multiple branch/location management</summary>
  Branches = 1 << 2,

  /// <summary>Team member management</summary>
  Users = 1 << 3,

  /// <summary>Purchase/supplier management</summary>
  Purchases = 1 << 4,

  /// <summary>Inter-branch inventory transfers</summary>
  InventoryTransfers = 1 << 5,

  /// <summary>Invoice generation and management</summary>
  Invoices = 1 << 6,

  /// <summary>Payment processing and tracking</summary>
  Payments = 1 << 7,

  /// <summary>Advanced reporting and analytics</summary>
  Reports = 1 << 8,

  /// <summary>Audit logs and activity tracking</summary>
  AuditLogs = 1 << 9,

  /// <summary>All features enabled (used for premium plans)</summary>
  All = Sales | Products | Branches | Users | Purchases | InventoryTransfers | Invoices | Payments | Reports | AuditLogs
}
